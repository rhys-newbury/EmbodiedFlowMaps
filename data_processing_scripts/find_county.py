import urllib3
import re

l = []
succ = 0
fail =  0
dup = 0
def add_correct_counties(lat_file):

    data = []
    with open(lat_file) as f:

        for index,line in enumerate(f):
            if index == 0:
                continue
            line_data = line.split(",")
            data.append(list(map(lambda x : x.strip(), line.split(","))))
        data = sorted(data, key=lambda x: (float(x[10]),float(x[11])))
    
        try:
            data = line.split(",")
            lat = data[10]
            long = data[11]
            #Skip all buildings with longitude or latitude of 0
            if (lat ==  '0.0' or long == '0.0'  or lat == '0' or long == '0'):

                fail += 1
            #Skip anything which is not numeric
            elif not lat.replace("-","").replace(".","").isdigit() and  not long.replace("-","").replace(".","").isdigit():
                fail += 1

            else:
                #Use api to get data
                request = "https://geo.fcc.gov/api/census/block/find?latitude=" + str(lat) + "&longitude=" + str(long) + "&format=xml"
                http = urllib3.PoolManager()
                r = http.request('GET', request)

                #Use Regex to match in result
                match = re.search('name="(.*?)"', str(r.data))

                county_name = match.group(1)

                line = line.strip()
                line += "," + county_name
                succ += 1

            l.append(line)


            if (succ+fail) % 10 == 0:
                print(succ,fail)

        except:
            pass

    f = '\n'.join(l)
    x = open("data_with_counties.csv", "w")
    x.write(f)





