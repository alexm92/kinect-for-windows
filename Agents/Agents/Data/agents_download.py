import json
import hashlib
import urllib

def get_md5(s):
    h = hashlib.md5(s)
    return h.hexdigest()

def download_image(url):
    ext = url.split('.')[-1]
    filename = 'Agents/%s.%s' % (get_md5(url), ext)
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
    f = open('agents.json', 'r')
    data = f.read()
    f.close()
    
    jdata = json.loads(data)
    for index, agent in enumerate(jdata):
        print 'Agent #%s: %s' % (index, agent['name'])
        filename = download_image(agent['image'])
        if filename is not None:
            agent['image'] = filename
        else:
            agent['image'] = ''

    to_json(jdata, 'new_agents.json')

    
    
