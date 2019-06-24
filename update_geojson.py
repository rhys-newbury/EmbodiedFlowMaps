us_state_abbrev = {
    'Alabama': 'AL',
    'Alaska': 'AK',
    'Arizona': 'AZ',
    'Arkansas': 'AR',
    'California': 'CA',
    'Colorado': 'CO',
    'Connecticut': 'CT',
    'Delaware': 'DE',
    'Florida': 'FL',
    'Georgia': 'GA',
    'Hawaii': 'HI',
    'Idaho': 'ID',
    'Illinois': 'IL',
    'Indiana': 'IN',
    'Iowa': 'IA',
    'Kansas': 'KS',
    'Kentucky': 'KY',
    'Louisiana': 'LA',
    'Maine': 'ME',
    'Maryland': 'MD',
    'Massachusetts': 'MA',
    'Michigan': 'MI',
    'Minnesota': 'MN',
    'Mississippi': 'MS',
    'Missouri': 'MO',
    'Montana': 'MT',
    'Nebraska': 'NE',
    'Nevada': 'NV',
    'New Hampshire': 'NH',
    'New Jersey': 'NJ',
    'New Mexico': 'NM',
    'New York': 'NY',
    'North Carolina': 'NC',
    'North Dakota': 'ND',
    'Ohio': 'OH',
    'Oklahoma': 'OK',
    'Oregon': 'OR',
    'Pennsylvania': 'PA',
    'Rhode Island': 'RI',
    'South Carolina': 'SC',
    'South Dakota': 'SD',
    'Tennessee': 'TN',
    'Texas': 'TX',
    'Utah': 'UT',
    'Vermont': 'VT',
    'Virginia': 'VA',
    'Washington': 'WA',
    'West Virginia': 'WV',
    'Wisconsin': 'WI',
    'Wyoming': 'WY',
}
out={}
inc={}
import re
for i in open('out.csv'):
   data = i.split(",")
   out[data[0]] = data[1].strip()

for i in open('inc.csv'):
   data = i.split(",")
   inc[data[0]] = data[1].strip()

import json
import os

print("hello")

filename = 'map_data/stateData.geojson'

print(inc)
print(out)

final = []

with open(filename, 'r') as f:
    for line in f:
        match = re.search('{"name":"([^"]+)"[^}]+}', line)
        if match:
            name = match.group(1)
            line = line.replace(match.group(0), '{"name":"'+name+'","outgoing":'+out[us_state_abbrev[name]]+',"incoming":'+inc[us_state_abbrev[name]]+'}')
            final.append(line)


out = open("map_data/newStateData.json", 'w')
out.write('\n'.join(final))
out.close()


#os.remove(filename)
#with open(filename, 'w') as f:
#    json.dump(data, f, indent=4)