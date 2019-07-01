import utm
data = []
for i in open('alabama.txt'):
        data.append(list(map(lambda x  : float(x.strip()), i.split(","))))
coords = []
for i in data:
        
        coords.append(utm.from_latlon(i[1],i[0]))
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
out = '\n'.join(list(map(lambda x : str(x[0]) + "," + str(x[1]), coords)))

file = open('coords.txt', 'w')
file.write(out)
file.close()
