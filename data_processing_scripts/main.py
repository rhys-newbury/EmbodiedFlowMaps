import combine_state_data

import find_county

import get_county_flow

import process_fac_file

vol_name = input("Enter Name Of Volume File")
#vol_name = "../UPS network data v2 2019-4-23/vol2.csv"

slc_name = input("Enter Name Of SLC File")
#slc_name = "../UPS network data v2 2019-4-23/slc2.csv"

lat_name = input("Enter Name Of Latitude File")
#lat_name = "../UPS network data v2 2019-4-23/lat2.csv"

fac_name = input("Enter Name Of Faciltiy File")
#fac_name = "../UPS network data v2 2019-4-23/fac.csv"

combine_state_data.generate_data(slc_name, vol_name)
print("Generated combine data")
find_county.add_correct_counties(lat_name)
print("Found counties using API")
process_fac_file.process_fac(fac_name)
print("Found capacties")
get_county_flow.get_county_flow(vol_name)