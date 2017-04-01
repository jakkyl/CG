import sys
import math
import queue
        
class Point:
    def __init__(self, x, y):
        self.x = x
        self.y = y

    def __str__(self, **kwargs):
        return "[{},{}]".format(self.x, self.y)

    def __lt__(self, other):
        return self.x < other.x and self.y <= other.y

    def __gt__(self, other):
        return self.x > other.x

    def __eq__(self, other):
        return self.x == other.x and self.y == other.y

    def __hash__(self):
        return hash(self.x) ^ hash(self.y)

    def __repr__(self):
        return str(self)

    def distance(self, point):
        return (point.x - self.x) ** 2 + (point.y - self.y) ** 2

class Node:
    def __init__(self, direction, laddersRemaining):
        self.direction = direction
        self.laddersRemaining = laddersRemaining

    def __str__(self, **kwargs):
        return "{} {}".format(self.direction, self.laddersRemaining)

# Auto-generated code below aims at helping you parse
# the standard input according to the problem statement.

# nb_floors: number of floors
# width: width of the area
# nb_rounds: maximum number of rounds
# exit_floor: floor on which the exit is found
# exit_pos: position of the exit on its floor
# nb_total_clones: number of generated clones
# nb_additional_elevators: ignore (always zero)
# nb_elevators: number of elevators
nb_floors, width, nb_rounds, exit_floor, exit_pos, nb_total_clones, nb_additional_elevators, nb_elevators = [int(i) for i in input().split()]
elevators = []
blockingClones = []
newPosition = None

graph = []
for p in range(width):
    inner = []
    for f in range(nb_floors):

        if p == exit_pos and f == exit_floor:
            inner.append(5)
        else:
            inner.append(0)
    graph.append(inner)

extraLadders = nb_additional_elevators
for i in range(nb_elevators):
    # elevator_floor: floor on which this elevator is found
    # elevator_pos: position of the elevator on its floor
    elevator_floor, elevator_pos = [int(j) for j in input().split()]
    elevators.append(Point(elevator_pos, elevator_floor))
    graph[elevator_pos][elevator_floor] = 4

def cleanupGraph():
    minX = minY = 10000
    maxW = maxY = -1

    global graph, width, nb_floors
    for p in range(width):    
        for f in range(nb_floors):
            if graph[p][f] > 0:
                if p > maxW: maxW = p
                if p < minX: minX = p
                if f > maxY: maxY = f
                if f < minY: minY = f

    for p in range(width):    
        for f in range(nb_floors):
            if p < minX: graph[p][f] = -1
            if p > maxW: graph[p][f] = -1
            if p < minY: graph[p][f] = -1
            if p < minY: graph[p][f] = -1
    return

def getDirection(currentPos, goalPosition, currentDirection):
    global blockingClones

    #print("CurrPos: {} GoalPos: {} CurrDir: {}\n{}".format(currentPos,
    #goalPosition, currentDirection, blockingClones), file=sys.stderr)
    if goalPosition.x < currentPos.x and currentDirection != 'LEFT' and len([c for c in blockingClones if c.x > currentPos.x and c.y == currentPos.y]) == 0:
        return 'LEFT'
    elif goalPosition.x > currentPos.x and currentDirection != 'RIGHT' and len([c for c in blockingClones if c.x < currentPos.x and c.y == currentPos.y]) == 0:
        return 'RIGHT'
    
    return currentDirection

def switchDirection(floor, pos):
    print("Switching: {} {}".format(floor,pos), file=sys.stderr)
    global blockingClones, graph

    blockingClones.append(Point(pos, floor))
    graph[pos][floor] = 1
    action = "BLOCK"   
    print(action)

    return

def elevatorsOnFloor(floor, pos):
    filtered = [e for e in elevators if e.y == floor]
    filtered.sort(key = lambda e: e.distance(pos))

    return filtered

def aStar(start, goal, node):
    closedSet = []
    openSet = queue.PriorityQueue()
    openSet.put((0, start))
    cameFrom = {}
    cameFrom[start] = None

    gScore = {}
    gScore[start] = 0
    dScore = {}
    dScore[start] = node
    
    while not openSet.empty():                
        current = openSet.get()[1]
        #print("Current: {} ".format(str(current)), file=sys.stderr)
        if current == goal: break

        closedSet.append(current)
        neighbors = getNeighbors(current)
        for neighbor in neighbors:                        
            #print("N: {} ".format(str(neighbor)), file=sys.stderr)
            tempScore = gScore[current] + cost(current, neighbor, dScore[current])
            #print("Score:{} C:{} N:{} L:{}".format(tempScore, current, neighbor,dScore[current]), file=sys.stderr)

            if neighbor not in gScore or tempScore < gScore[neighbor]:
               # print("Node: {} {} {} {}".format(str(dScore[current]), current, neighbor, tempScore), file=sys.stderr)
                gScore[neighbor] = tempScore
                dScore[neighbor] = Node(dScore[current].direction if getDirection(current, neighbor, dScore[neighbor if neighbor in dScore else current].direction) == dScore[current].direction \
                                        else 'RIGHT' if dScore[current].direction == 'LEFT' else 'LEFT', dScore[current].laddersRemaining)
                cameFrom[neighbor] = current
                
                #print("DScore:{} C:{} N:{} L:{}".format(tempScore, current, neighbor,dScore[neighbor]), file=sys.stderr)
                #print("Score: {} {}->{}".format(tempScore,
                #current,neighbor),file=sys.stderr)
                openSet.put((tempScore + hueristic(neighbor, goal, dScore[neighbor]), neighbor))

    return getPath(cameFrom, start, goal)

def getPath(cameFrom, start, goal):
    current = goal
    path = [current]
    
    s = ''
    for k,v in cameFrom.items():
        s += "{},{} ".format(k,v)
    #print("getPath-current: {} ".format(s), file=sys.stderr)
    while current != start:
        current = cameFrom[current]
        path.append(current)

    path.reverse()
    return path

def getNeighbors(current):
    adjacent = []

    global width, nb_floors, graph
    if current.x - 1 > 0 and checkNeighbor(current.x - 1, current.y):
        adjacent.append(Point(current.x - 1, current.y))
    if current.x + 1 < width - 1 and checkNeighbor(current.x + 1, current.y):
        adjacent.append(Point(current.x + 1, current.y))
    if current.y + 1 < nb_floors and checkNeighbor(current.x, current.y + 1):
        adjacent.append(Point(current.x, current.y + 1))

    return adjacent

def checkNeighbor(x, y):
    global graph

    val = graph[x][y]
    if  val == -1: return False

    return True

def cost(start, goal, node):
    cost = 10
    floorChange = goal.y != start.y

    global extraLadders, nb_additional_elevators
    onFloor = elevatorsOnFloor(start.y, start)
    if floorChange and graph[start.x][start.y] != 4:
        if node.laddersRemaining > 0 and (len(onFloor) == 0 or extraLadders > 0) and nb_additional_elevators > 0:
            cost *= 5
            node.laddersRemaining -= 1
        else: cost = sys.maxsize
    #elif floorChange and graph[start.x][start.y] == 4:
        #print("elevator, reduce cost: {} for {}->{}".format(cost, start,
        #goal), file=sys.stderr)
        #cost /= 3
    elif not floorChange and graph[start.x][start.y] == 4:
        #if you are on an elevator, you must go up
        cost = sys.maxsize
    elif getDirection(start, goal, node.direction) != node.direction:
        #print("changing direction, increase cost: {} for {}->{}".format(cost,
        #start, goal), file=sys.stderr)
        cost *= 5

    return cost

def hueristic(start, goal, node):
    multiplier = 1
    if getDirection(start, goal, node.direction) != node.direction:
        multiplier = 5

    return (abs(start.x - goal.x)*multiplier + abs(start.y - goal.y)) * 10

buildingElevator = 0
turn = 0
for i in range(nb_floors):
    if len(elevatorsOnFloor(i, Point(0,i))) == 0: extraLadders -= 1

# game loop
while True:
    turn += 1
    # clone_floor: floor of the leading clone
    # clone_pos: position of the leading clone on its floor
    # direction: direction of the leading clone: LEFT or RIGHT
    clone_floor, clone_pos, direction = input().split()
    clone_floor = int(clone_floor)
    clone_pos = int(clone_pos)
    graph[clone_pos][clone_floor] = 10 if turn == 1 else max(1, graph[clone_pos][clone_floor])
    cleanupGraph()
    clone = Point(clone_pos, clone_floor)

    print("Turn: {}\nCurrent Position/Floor:{} / {} EL:{}".format(turn, clone_pos, clone_floor, extraLadders), file=sys.stderr)    
    #for i in range(nb_floors - 1,-1,-1):
    #    s = ''
    #    for x in range(width):
    #        s += " {}".format(graph[x][i])
    #    print("{}".format(s), file=sys.stderr)
        
    action = "WAIT"
    if clone_pos == -1 or buildingElevator > 0: 
        buildingElevator = 0
        print(action)
        continue

    # are we on the exit floor?
    if clone_floor == exit_floor:
        #are we going the right way?
        if getDirection(clone, Point(exit_pos, exit_floor), direction) != direction:
            switchDirection(clone_floor, clone_pos)
            continue
        print(action)
        continue

    print("aStar current: {} goal: {}".format(Point(clone_pos, clone_floor), Point(exit_pos, exit_floor)), file=sys.stderr)
    path = aStar(Point(clone_pos, clone_floor), Point(exit_pos, exit_floor), Node(direction, nb_additional_elevators))
    print("Path {}".format(str(path)), file=sys.stderr)
    if len(path) > 1 and getDirection(clone, path[1], direction) != direction:
        switchDirection(clone_floor, clone_pos)
        continue

    #go through path until we switch floors and add an elevator
    filteredElevators = elevatorsOnFloor(clone_floor, path[1])
    if nb_additional_elevators > 0:
        for i in range(len(path)):
            if path[i].y == clone_floor + 1:
                newPosition = path[i - 1]
                break
    
    if newPosition:        
        print("newPos: {} at {}".format(newPosition, clone_pos), file=sys.stderr)
        if newPosition.x == clone_pos and len([e for e in elevators if e.y == clone_floor and e.x == clone_pos]) == 0:
            action = "ELEVATOR"   
            elevators.append(Point(clone_pos,clone_floor))            
            graph[clone_pos + (newPosition.x - clone_pos)][clone_floor] = 4
            print("{}".format(action), file=sys.stderr)        
            buildingElevator = 1
            nb_additional_elevators -= 1                 
            if len(elevatorsOnFloor(clone_floor, newPosition)) == 0: extraLadders -= 1
            newPosition = None
        elif clone_floor == newPosition.y and getDirection(clone, newPosition, direction) != direction:
            switchDirection(clone_floor, clone_pos)
        print(action)
        continue

    filteredElevators = elevatorsOnFloor(clone_floor, path[1])
    if len(filteredElevators) > 0:      
        print("elevators on floor: {} at {}".format(newPosition, clone_pos), file=sys.stderr)  
        switch = False
        for el in filteredElevators:
            if getDirection(clone, el, direction) != direction:
                switch = True
                switchDirection(clone_floor, clone_pos)
            break
        if switch: continue
        


    # Write an action using print
    # To debug: print("Debug messages...", file=sys.stderr)

    # action: WAIT or BLOCK
    print(action)
