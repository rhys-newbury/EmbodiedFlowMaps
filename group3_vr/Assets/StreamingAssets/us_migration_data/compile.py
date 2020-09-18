import glob
from collections import defaultdict
l = glob.glob("C:\Users\FIT3161\Desktop\group3\group3_vr\us_migration_data\state_data\*")

county_incoming = defaultdict(int)
county_outgoing = defaultdict(int)

for i in l:

    for line in open(i):
        data = list(filter(lambda x : x != "", line.strip().split(",")))

        if len(data) != 16:
            continue
        
        state_to = data[4].strip()
        county_to = data[5].strip()

        state_from = data[6].strip()
        county_from = data[7].strip()

        data = int(data[8])

        county_incoming[state_to + "," + county_to] += data
        county_outgoing[state_from + "," + county_from] += data


cin = map(lambda i : ",".join(map(str,i)),  county_incoming.items())
cout = map(lambda i : ",".join(map(str,i)),  county_outgoing.items())

f1 = open("C:\Users\FIT3161\Desktop\group3\group3_vr\us_migration_data\county_inc.txt","w")
f1.write("\n".join(cin))
f1.close()

f2 = open("C:\Users\FIT3161\Desktop\group3\group3_vr\us_migration_data\county_out.txt","w")
f2.write("\n".join(cout))
f2.close()
