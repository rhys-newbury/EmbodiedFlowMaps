import urllib3
import re
d = []
list = []
succ = 0
duplicate = 0
fail =  0
header = ''
prevLat = -1
prevLong = -1
state_name  = ""
county_name = ""

with open('UPS network data v2 2019-4-23/lat2.csv') as f:

    for line in f:
        if "Facility Latitude" in line:
                header = line
                continue
        d.append(line.split(","))

    d = sorted(d, key=lambda x:(float(x[10]), float(x[11])))

    for data in d:
        try:
            line = ",".join(data)
            lat = data[10]
            long = data[11]               

            if (lat ==  '0.0' or long == '0.0'  or lat == '0' or long == '0'):

                fail += 1

            elif not lat.replace("-","").replace(".","").isdigit() and  not long.replace("-","").replace(".","").isdigit():
                fail += 1

            elif prevLong == long and prevLat == lat:

                line = line.strip()


                line += "," + state_name + "," + county_name
                duplicate += 1

                

            else:
                request = "https://geo.fcc.gov/api/census/block/find?latitude=" + str(lat) + "&longitude=" + str(long) + "&format=xml"

                http = urllib3.PoolManager()
                r = http.request('GET', request)

                match = re.findall('name="(.*?)"', str(r.data))


                county_name = match[0]
                state_name = match[1]

                line = line.strip()
                line += "," + state_name + "," + county_name
                succ += 1

                prevLong = long
                prevLat = lat

            list.append(line)


            if (succ+fail+duplicate) % 10 == 0:
                print(duplicate, succ,fail)





        except:
            pass

f = '\n'.join(list)
x = open("data_with_counties.csv", "w")
x.write(f)





