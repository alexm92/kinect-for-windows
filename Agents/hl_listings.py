from BeautifulSoup import BeautifulSoup
import urllib2
import json
import pprint

domain = 'http://hlresidential.com'
VALID_TAGS = ['br']

def sanitize_html(value):
    soup = BeautifulSoup(value)
    for tag in soup.findAll(True):
        if tag.name not in VALID_TAGS:
            tag.hidden = True
    return soup.renderContents()

def get_listings_url():
    listings_url = set()
    
    ## sales
    url = '%s/sales-grid?id=&price[min]=&price[max]=&bed=&bed_max=&bath=All&items_per_page=200' % domain
    urls = _get_urls(url)
    listings_url.update(urls)
    
    ## rentals
    for page in xrange(0, 15):
        url = '%s/rental-grid?id=&price[min]=&price[max]=&bed=&bed_max=&bath=All&&&&items_per_page=200&page=%s' % (domain, page)
        urls = _get_urls(url)
        listings_url.update(urls)

    return listings_url

def _get_urls(url):
    urls = set()
    html = urllib2.urlopen(url).read()
    listing_start = html.find('<td')
    listing_start = html.find('<div class="field-content">', listing_start)
    while listing_start != -1:
        url_start = html.find('<a href="/listing', listing_start) + len('<a href="')
        url_end = html.find('"', url_start)
        url = html[url_start : url_end]

        urls.add(url)
        
        listing_start = html.find('<td', url_end)
        listing_start = html.find('<div class="field-content">', listing_start)

    return urls

def get_listing_details(url):
    url = '%s%s' % (domain, url)
    html = urllib2.urlopen(url).read()

    listing = {
        'title': '',
        'address': '',
        'images': [],
        'map': '',
        'price': '',
        'type': '',
        'bedrooms': '',
        'bathrooms': '',
        'size': '',
        'amenities': [],
        'maintenance': '',
        'description': '',
        'subway': [],
        'agent': '',
    }

    title_start = html.find('<h1 id="list-title">') + len('<h1 id="list-title">')
    title_end = html.find('</', title_start)
    listing['title'] = sanitize_html(html[title_start : title_end])

    address_start = html.find('<div class="list_address">')
    address_end = html.find('</div>', address_start)
    listing['address'] = sanitize_html(html[address_start : address_end]).replace('\n', '')

    images_start = html.find('<div class="fotorama"')
    images_end = html.find('</div>', images_start)
    images_content = html[images_start : images_end]
    image_start = images_content.find('<img src="')
    while image_start != -1:
        image_start += len('<img src="')
        image_end = images_content.find('"', image_start)
        image = images_content[image_start : image_end]
        if image not in listing['images']:
            listing['images'].append(image)
        image_start = images_content.find('<img src="', image_end)

    map_start = html.find('<img src="http://maps')
    if  map_start != -1:
        map_start += len('<img src="')
        map_end = html.find('"', map_start)
        listing['map'] = html[map_start : map_end]

    price_start = html.find('<div class="list_price">')
    if price_start != -1:
        price_end = html.find('</', price_start)
        listing['price'] = sanitize_html(html[price_start : price_end]).replace('\n', '')

    type_start = html.find('<div class="listing_row_label">Type</div>')
    if type_start != -1:
        type_start += len('<div class="listing_row_label">Type</div>')
        type_end = html.find('</', type_start)
        listing['type'] = sanitize_html(html[type_start : type_end]).replace('\n', '').replace('\t', '')

    bedrooms_start = html.find('<div class="listing_row_label">Bedrooms</div>')
    if bedrooms_start != -1:
        bedrooms_start += len('<div class="listing_row_label">Bedrooms</div>')
        bedrooms_end = html.find('</', bedrooms_start)
        listing['bedrooms'] = sanitize_html(html[bedrooms_start : bedrooms_end])

    bathrooms_start = html.find('<div class="listing_row_label">Bathrooms</div>')
    if bathrooms_start != -1:
        bathrooms_start += len('<div class="listing_row_label">Bathrooms</div>')
        bathrooms_end = html.find('</', bathrooms_start)
        listing['bathrooms'] = sanitize_html(html[bathrooms_start : bathrooms_end])

    size_start = html.find('<div class="listing_row_label">Interior size (sq. ft.)</div>')
    if size_start != -1:
        size_start += len('<div class="listing_row_label">Interior size (sq. ft.)</div>')
        size_end = html.find('</', size_start)
        listing['size'] = sanitize_html(html[size_start : size_end]).replace('\n', '').replace('\t', '')

    amenities_start = html.find('<div class="list_features">')
    if amenities_start != -1:
        amenities_end = html.find('</div>', amenities_start)
        amenities = html[amenities_start : amenities_end]
        a_start = amenities.find('<li>')
        while a_start != -1:
            a_start += len('<li>')
            a_end = amenities.find('</', a_start)
            a = amenities[a_start : a_end]
            listing['amenities'].append(a)
            a_start = amenities.find('<li>', a_end)

    maintenance_start = html.find('<div class="listing_row_label">Maintenance fee</div>')
    if maintenance_start != -1:
        maintenance_start += len('<div class="listing_row_label">Maintenance fee</div>')
        maintenance_end = html.find('</', maintenance_start)
        listing['maintenance'] = sanitize_html(html[maintenance_start : maintenance_end]).replace('\n', '')

    description_start = html.find('field-type-text-with-summary')
    if description_start != -1:
        description_start = html.find('<div class="field-items">', description_start)
        description_end = html.find('<div class="more_body">', description_start)
        listing['description'] = sanitize_html(html[description_start : description_end])

    subway_start = html.find('<span class="subway_')
    while subway_start != -1:
        subway_start += len('<span class="subway_')
        subway_end = html.find('"', subway_start)
        listing['subway'].append(html[subway_start : subway_end])
        subway_start = html.find('<span class="subway_', subway_end)

    agent_start = html.find('<div class="listing_row_descr">Agents</div>')
    if agent_start != -1:
        agent_start = html.find('<a href="/', agent_start) + len('<a href="/')
        agent_end = html.find('"', agent_start)
        listing['agent'] = html[agent_start : agent_end]

    return listing

def data_to_json(filename, data):
    f = open(filename, 'w')
    jdata = json.dumps(data)
    f.write(jdata)
    f.close()

if __name__ == '__main__':
    listings_url = get_listings_url()
    listings = []
  
    print 'Got', len(listings_url), 'url(s).'

    for url in listings_url:
        print 'Parsing', url
        listing = get_listing_details(url)
        #pprint.pprint(listing)
        listings.append(listing)

    print 'Export to JSON.'
    data_to_json('listings.json', listings)
    print 'Done!'
