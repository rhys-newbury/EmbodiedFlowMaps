import glob
from collections import defaultdict
l = glob.glob("C:\Users\FIT3161\Desktop\group3\group3_vr\us_migration_data\state_data\*")

county_incoming = defaultdict(int)
county_outgoing = defaultdict(int)

final_data = []

for i in l:

    for line in open(i):
        data = list(filter(lambda x : x != "", line.strip().split(",")))

        if len(data) != 16:
            continue
        
        state_to = data[4].strip()
        county_to = data[5].strip()

        state_from = data[6].strip()
        county_from = data[7].strip()

        flow = data[8].strip()

        if flow == "0":
            continue

        final_data.append(",".join([state_to, county_to, state_from, county_from, flow]))



f1 = open("C:\Users\FIT3161\Desktop\group3\group3_vr\us_migration_data\county_flow.txt","w")
f1.write("\n".join(final_data))
f1.close()
