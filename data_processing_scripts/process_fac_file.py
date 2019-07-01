import collections

d = collections.defaultdict(int)


for index, line in enumerate(open('../UPS network data v2 2019-4-23/fac.csv')):

    if index == 0:
        continue

    data = list(map(lambda x : x.strip(), line.split(",")))

    time = float(data[4][0] + "." + data[4][1:])
    total = time * float(data[3])

    d[data[1]] += total

output = '\n'.join(map(lambda i : (str(i[0]) + "," + str(i[1])), d.items()))

file = open("capacities.csv", 'w')
file.write(output)
file.close()