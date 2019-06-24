import urllib3
import re

list = []
succ = 0
fail =  0
with open('UPS network data v2 2019-4-23/lat2.csv') as f:

    for line in f:
        try:
            data = line.split(",")
            lat = data[10]
            long = data[11]

            if (lat ==  '0.0' or long == '0.0'  or lat == '0' or long == '0'):

                fail += 1

            elif not lat.replace("-","").replace(".","").isdigit() and  not long.replace("-","").replace(".","").isdigit():
                fail += 1

            else:
                request = "https://geo.fcc.gov/api/census/block/find?latitude=" + str(lat) + "&longitude=" + str(long) + "&format=xml"

                http = urllib3.PoolManager()
                r = http.request('GET', request)

                match = re.search('name="(.*?)"', str(r.data))

                county_name = match.group(1)

                line = line.strip()
                line += "," + county_name
                succ += 1

            list.append(line)


            if (succ+fail) % 10 == 0:
                print(succ,fail)





        except:
            pass

f = '\n'.join(list)
x = open("data_with_counties.csv", "w")
x.write(f)





