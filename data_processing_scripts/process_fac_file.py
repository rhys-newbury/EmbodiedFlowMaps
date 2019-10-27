import collections

d = collections.defaultdict(int)

def process_fac(fac):

    for index, line in enumerate(open(fac)):

        if index == 0:
            continue

        data = list(map(lambda x : x.strip(), line.split(",")))

        time = float(data[4][0] + "." + data[4][1:])
        total = time * float(data[3])

        d[data[1]] += total

        print(d)
    output = '\n'.join(map(lambda i : (str(i[0]) + "," + str(i[1])), d.items()))

    file = open("capacities.csv", 'w')
    file.write(output)
    file.close()