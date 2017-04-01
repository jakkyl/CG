import sys
import math

# Auto-generated code below aims at helping you parse
# the standard input according to the problem statement.

# w: width of the building.
# h: height of the building.
w, h = [int(i) for i in input().split()]
maxJumps = int(input())  # maximum number of turns before game over.
x0, y0 = [int(i) for i in input().split()]

newRangeX = range(w)
newRangeY = range(h)

print("{} {}".format(w, h), file=sys.stderr)

lastPosX = x0
lastPosY = y0    

def processWarmer(x, x1, y, y1):
    global newRangeX, newRangeY

    if x1 != x:
        newRangeX = [i for i in newRangeX if abs(i - x1) < abs(i - x)]        
    else:
        newRangeY = [i for i in newRangeY if abs(i - y1) < abs(i - y)]
    return

def newPosition(currPos, newRange, maxVal):
    pos = newRange[0] + newRange[-1] - currPos

    if pos == currPos:
        pos += 1
    pos = max(0, min(pos, maxVal))

    if currPos == 0 or currPos == maxVal:
        pos = int((pos + currPos) / 2)

        if pos == 0: pos = 1
        elif pos == w - 1: pos -= 1
    return pos

# game loop
while True:
    bomb_dir = input()  # Current distance to the bomb compared to previous distance (COLDER, WARMER,
                        # SAME or UNKNOWN)
                  
    print("{}".format(bomb_dir), file=sys.stderr)
        
    if len(newRangeX) == 1 and newRangeX[0] != x0:
        x0 = newRangeX[0]
        lastPosX = x0
        print("{} {}".format(x0, y0))
        continue
    elif len(newRangeY) == 1 and newRangeY[0] != y0:
        y0 = newRangeY[0]
        lastPosY = y0
        print("{} {}".format(x0, y0))
        continue

    if bomb_dir == 'COLDER':
        processWarmer(x0,lastPosX, y0, lastPosY)
    elif bomb_dir == 'WARMER':   
        processWarmer(lastPosX, x0, lastPosY, y0)
    elif bomb_dir == 'SAME':
        if x0 != lastPosX:
            newRangeX = [int((newRangeX[0] + newRangeX[-1]) / 2)]
        else:
            newRangeY = [int((newRangeY[0] + newRangeY[-1]) / 2)]
    
    lastPosX = x0
    lastPosY = y0

    if len(newRangeX) > 1:
        x0 = newPosition(x0, newRangeX, w - 1)        
    else:            
        y0 = newPosition(y0, newRangeY, h - 1)
                    
    print("L/C: {},{} / {},{}".format(lastPosX, lastPosY, x0, y0), file=sys.stderr)
    print("New Range: {} {}".format(str(newRangeX),str(newRangeY)), file=sys.stderr)
    print("New Pos: {} {}".format(x0,y0), file=sys.stderr)
    # Write an action using print
    # To debug: print("Debug messages...", file=sys.stderr)
    
    
    # the location of the next window Batman should jump to.
    print("{} {}".format(x0, y0))
