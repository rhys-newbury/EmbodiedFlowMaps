import collections

d = {}

inc = collections.defaultdict(int)
out = collections.defaultdict(int)
flow =collections.defaultdict(int)

with open('data_with_counties.csv') as f:

    for line in f:

        data = line.split(",")
        try:
            if data[12] == "US":
                d[data[0].strip()] = data[13].strip() + "," + data[14].strip()
        except:
            print("fail")
            
       
with open('../UPS network data v2 2019-4-23/vol2.csv') as f:
    
    for line in f:
        data = line.split(",")
    
        if data[0].strip() == "US" and data[2].strip() == "US":
            if data[1].strip() in d and data[3].strip() in d:
                inc[d[data[1].strip()]] += float(data[4].strip())
                out[d[data[3].strip()]] += float(data[4].strip())
                flow[d[data[3].strip()] + "," + d[data[1].strip()]] += float(data[4].strip())   
                


x = open('county_in.csv', 'w')
x.write("\n".join([",".join([key, str(val)]) for key, val in inc.items()]))
x.close()

x = open('county_out.csv', 'w')
x.write("\n".join([",".join([key, str(val)]) for key, val in out.items()]))
x.close()

x = open('county_flow.csv', 'w')
x.write("\n".join([",".join([key, str(val)]) for key, val in flow.items()]))
x.close()


