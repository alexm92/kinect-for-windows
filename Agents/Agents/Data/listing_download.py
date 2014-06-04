import json
import hashlib
import urllib

def get_md5(s):
    h = hashlib.md5(s)
    return h.hexdigest()

def download_image(url):
    ext = url.split('.')[-1]
    filename = 'listing_images/%s.%s' % (get_md5(url), ext)
    try:
        urllib.urlretrieve(url, filename)
    except Exception as e:
        print e
        return None
    return filename

def to_json(data, filename):
    jdata = json.dumps(data)
    f = open(filename, 'w')
    f.write(jdata)
    f.close()

if __name__ == '__main__':
    f = open('listings.json', 'r')
    data = f.read()
    f.close()
    
    jdata = json.loads(data)
    n = 5
    for index, listing in enumerate(jdata):
        images = []
        print 'Listing #%s: %s' % (index, listing['title'])
        for image in listing['images']:
            filename = download_image(image)
            if filename is not None:
                images.append(filename)
        listing['images'] = images

    to_json(jdata, 'new_listings.json')

    
    
