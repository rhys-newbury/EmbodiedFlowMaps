import collections

def generate_data(slc_name, vol2_name):
    d = {}

    inc = collections.defaultdict(int)
    out = collections.defaultdict(int)
    flow =collections.defaultdict(int)

    with open(slc_name) as f:

        for line in f:

            data = line.split(",")

            if len(data) == 10 and all(map(lambda x : len(x) > 0, data)) and data[5] == "US":
                d[data[0].strip()] = data[6].strip()

        
    with open(vol2_name) as f:
        for line in f:
            data = line.split(",")

            if len(data) == 5 and all(map(lambda x : len(x) > 0, data)) and data[0].strip() == "US" and data[2].strip() == "US":
                if data[1].strip() in d and data[3].strip() in d:
                    inc[d[data[1].strip()]] += float(data[4].strip())
                    out[d[data[3].strip()]] += float(data[4].strip())
                    flow[d[data[3].strip()] + "," + d[data[1].strip()]] += float(data[4].strip())                


    x = open('inc.csv', 'w')
    x.write("\n".join([",".join([key, str(val)]) for key, val in inc.items()]))
    x.close()

    x = open('out.csv', 'w')
    x.write("\n".join([",".join([key, str(val)]) for key, val in out.items()]))
    x.close()


    x = open('flow.csv', 'w')
    x.write("\n".join([",".join([key, str(val)]) for key, val in flow.items()]))
    x.close()


