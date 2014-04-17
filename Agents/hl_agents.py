import urllib2
import json

domain = 'http://hlresidential.com'

def get_agents_url():
    agents_url = []
    html = urllib2.urlopen('%s/agents' % domain).read()
    
    agent_start = html.find('views-field-field-b-photo')
    while agent_start != -1:
        agent_start = html.find('<a href="', agent_start) + len('<a href="')
        agent_end = html.find('"', agent_start)
        agent = html[agent_start : agent_end]

        if agent not in agents_url:
            agents_url.append(agent)

        agent_start = html.find('views-field-field-b-photo', agent_start)

    return agents_url

def get_agent_info(url):
    url = '%s%s' % (domain, url)
    html = urllib2.urlopen(url).read()
    agent = {
        'name': '',
        'image': '',
        'phone': '',
        'email': '',
        'description': '',
        'license': '',
        'university': '',
        'neighbourhoods': [],
        'languages': [],
    }

    img_start = html.find('<img typeof="foaf:Image" src="')
    if img_start != -1:
        img_start += len('<img typeof="foaf:Image" src="')
        img_end = html.find('"', img_start)
        agent['image'] = html[img_start : img_end]

    phone_start = html.find('field-name-field-bio-phone')
    if phone_start != -1:
        phone_start = html.find('<div class="field-item even">', phone_start) + len('<div class="field-item even">')
        phone_end = html.find('</div>', phone_start)
        agent['phone'] = html[phone_start : phone_end]

    email_start = html.find('field-name-field-b-email')
    if email_start != -1:
        email_start = html.find('<a href="mailto:', email_start) + len('<a href="mailto:')
        email_end = html.find('"', email_start)
        agent['email'] = html[email_start : email_end]

    name_start = html.find('<h1 id="bio-title">')
    if name_start != -1:
        name_start += len('<h1 id="bio-title">')
        name_end = html.find('</h1>', name_start)
        agent['name'] = html[name_start : name_end]

    license_start = html.find('field-name-field-b-license')
    if license_start != -1:
        license_start = html.find('<div class="field-item even">', license_start) + len('<div class="field-item even">')
        license_end = html.find('</div>', license_start)
        agent['license'] = html[license_start : license_end]

    description_start = html.find('field-name-body')
    if description_start != -1:
        description_start = html.find('<div class="field-item even" property="content:encoded">', description_start) + len('<div class="field-item even" property="content:encoded">')
        description_end = html.find('</div>', description_start)
        agent['description'] = html[description_start : description_end].replace('<p>', '').replace('</p>', '\n').replace('<br />', '\n')

    agent['languages'] = []
    lang_start = html.find('<div class="bio_lang">')
    if lang_start != -1:
        lang_start = html.find('<span title', lang_start)
        while lang_start != -1:
            lang_start = html.find('>', lang_start) + 1
            lang_end = html.find('</', lang_start)
            lang = html[lang_start : lang_end]
            agent['languages'].append(lang)
            lang_start = html.find('<span title', lang_start)

    agent['neighbourhoods'] = []
    ng_start = html.find('field-name-field-b-neighborhood')
    if ng_start != -1:
        ng_start = html.find('<li class="taxonomy', ng_start)
        while ng_start != -1:
            ng_start = html.find('>', ng_start) + 1
            ng_end = html.find('</', ng_start)
            ng = html[ng_start : ng_end]
            agent['neighbourhoods'].append(ng)
            ng_start = html.find('<li class="taxonomy', ng_start)

    univ_start = html.find('field-name-field-b-uni')
    if univ_start != -1:
        univ_start = html.find('<div class="field-item even">', univ_start) + len('<div class="field-item even">')
        univ_end = html.find('</', univ_start)
        agent['university'] = html[univ_start : univ_end]
    

    return agent

def export_to_json(filename, data):
    f = open(filename, 'w')
    jdata = json.dumps(data)
    f.write(jdata)
    f.close()

if __name__ == '__main__':
    url_list = get_agents_url()
    agents = []
    for url in url_list:
        print 'Parsing', url
        agent = get_agent_info(url)
        agents.append(agent)

    export_to_json('agents.json', agents)
    print 'Done!!'
