import re, utm, math
min0 = float('inf')
min1 = float('inf')
total = []
states = []
def convert(latitude, longitude):
        mapWidth = 2000
        mapHeight = 1000

        x = (longitude+180)*(mapWidth/360)
        
        latRad = latitude*math.pi/180

        mercN = math.log(math.tan((math.pi/4)+(latRad/2)))
        y = (mapHeight/2)-(mapWidth*mercN/(2*math.pi))

        return [x,y]
        
for line in open("data.txt"):
    match1 = re.search('"name":"(.*?)"', line)

    match2 = re.search(',"coordinates":\[\[(.*?)\]\]', line)

    name = match1.group(1)
    coordinates = re.findall("\[(.*?)\],", match2.group(1))

    #For each line, cast each part to float before converting to utm coordinates
    coordinates = list(map(lambda i : convert(i[1],i[0]), list(map(lambda i : list(map(lambda x  : float(x.strip()), i.split(","))), coordinates))))
    total.append([name, coordinates])

    
    
    #min0 = min(min0, min(coordinates, key=lambda e: int(e[0]))[0])
    #min1 = min(min1, min(coordinates, key=lambda e: int(e[1]))[1])
    
for line in total:
    state = line[0]
    coordinates = line[1]

    coordinates = map(lambda x : [str(x[0]), str(x[1])], coordinates)

    output = '\n'.join(list(map(lambda x : ','.join(x), coordinates)))
    file = open(state + ".csv", 'w')
    file.write(output)
    file.close()



    

    
    
