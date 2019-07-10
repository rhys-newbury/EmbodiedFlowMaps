import glob
max_dist = 0
for file in glob.glob("*.csv"):
    l = []
    
    A = 0
    Cx = 0
    Cy = 0
    x = []
    y = []
    for line in open(file):

        data = list(map(float, line.split(",")))
        x.append(data[0]/100)
        y.append(data[1]/100)
    x.append(x[0])
    y.append(y[0])
    for i in range(0, len(x)-1):
        A += x[i]*y[i+1] - x[i+1]*y[i]
        Cx += (x[i]+x[i+1]) * (x[i]*y[i+1] - x[i+1]*y[i])
        Cy += (y[i]+y[i+1]) * (x[i]*y[i+1] - x[i+1]*y[i])
        
    A *= 0.5
    Cx *= 1/(6*A)
    Cy *= 1/(6*A)

    max_dist = max(max_dist, max(list(map(lambda data : (data[0]**2 + data[1]**2)**0.5, zip(x,y)))))
    
    print(file, Cx, Cy)
    

    

    
    
