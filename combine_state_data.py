import collections

d = {}

inc = collections.defaultdict(int)
out = collections.defaultdict(int)


with open('UPS network data v2 2019-4-23/slc2.csv') as f:

    for line in f:

        data = line.split(",")

        if data[5] == "US":
            d[data[0].strip()] = data[6].strip()


with open('UPS network data v2 2019-4-23/vol2.csv') as f:
    for line in f:
        data = line.split(",")

        if data[0].strip() == "US" and data[2].strip() == "US":
            if data[1].strip() in d and data[3].strip() in d:
                inc[d[data[1].strip()]] += float(data[4].strip())
                out[d[data[3].strip()]] += float(data[4].strip())


x = open('inc.csv', 'w')
x.write("\n".join([",".join([key, str(val)]) for key, val in inc.items()]))
x.close()

x = open('out.csv', 'w')
x.write("\n".join([",".join([key, str(val)]) for key, val in out.items()]))
x.close()


