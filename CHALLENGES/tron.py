import sys
import math
from enum import Enum
from _collections import deque

class State(Enum):
    EMPTY = 0
    WALL = 1
    PLAYER = 2
    
class Point:
    def __init__(self, x, y):
        self.x = x
        self.y = y
        return

    def direction(self, p):
        if p.x > self.x: return 'RIGHT'
        if p.x < self.x: return 'LEFT'
        if p.y < self.y: return 'UP'        
        if p.y > self.y: return 'DOWN'

        return 'LEFT'

    def distance(self, p):
        return abs(p.x - self.x) + abs(p.y - self.y)

    def __str__(self, **kwargs):
        return "[{},{}]".format(self.x, self.y)

class Player(Point):
    def __init__(self, id, x0, y0, x1, y1):
        self.id = id
        self.head = Point(x0, y0)
        self.tail = Point(x1, y1)

        super().__init__(x1,y1)
        return 
            

class Node(Point):
    def __init__(self, x, y, state):
        self.state = state
        return super().__init__(x, y)

    def neighbors(self, graph):
        n = []

        if self.checkNeighbor(self.x - 1, self.y) : n.append(graph[self.x - 1][self.y])
        if self.checkNeighbor(self.x + 1, self.y) : n.append(graph[self.x + 1][self.y])
        if self.checkNeighbor(self.x, self.y - 1) : n.append(graph[self.x][self.y - 1])
        if self.checkNeighbor(self.x, self.y + 1) : n.append(graph[self.x][self.y + 1])

        return n

    def checkNeighbor(self, x, y):
        if x < 0 or x > 29 or y < 0 or y > 19: return False

        n = graph[x][y]
        if n.state == State.WALL or n.state == State.PLAYER: return False

        return True
        
    def score(self, graph, depth, maxPlayer):       
        #maxScore = depth
        #myScore = 0
        #for i in range(0, len(players), 2):
        #    p1Nodes = availableNodes(graph, graph[players[i].x][players[i].y])
        #    p2Nodes = availableNodes(graph, graph[players[i + 1].x][players[i
        #    + 1].y])
        #    if maxPlayer and len(p1Nodes) == 0:
        #    score = sum([node.distance(player) for node in nodes])
        #    p1Nodes = []
        #    if maxPlayer and player.id == p:
        #        myScore = score
        #        score = 0
        #    else:
        #        score = max(score, maxScore)
        
        #s = []
        #for i in range(20):
        #    for x in range(30):
        #        if graph[x][i].state != State.EMPTY:
        #            s.append("[{}]".format(graph[x][i].state.value))
        #print("{}".format(s), file=sys.stderr)

        player = self
        flatGraph = [node for node in sum(graph, []) if node.state == State.PLAYER and node != player]
        #others = [pl for pl in flatGraph if pl != player]

        v = voronoi(graph, player, flatGraph[0])
        #print("voronoi: {}\n{} ".format(len(v[0]), len(v[1])), file=sys.stderr)
        
        #myNodes = availableNodes(graph, graph[player.x][player.y])
        #theirNodes = []
        #distance = 0
        
        ##print("graph: {} ".format(flatGraph), file=sys.stderr)
        #for pl in others:
        #    nodes = availableNodes(graph, graph[pl.x][pl.y])
        #    distance += sum([node.distance(pl) for node in nodes])
        #    theirNodes = [x for x in nodes if player.distance(x) >
        #    pl.distance(x)]

        #myNodes = [x for x in myNodes if player.distance(x) <
        #others[0].distance(x)]

        score1 = (10000 * len(v[0]) - 10 * len(v[1])) / (depth + 1)
        #score2 = 1000 * (len(v[0]) - len(v[1]))#score / (depth + 1) #if maxPlayer else -score
        #print("Score1: {} for {}".format(score1, score2), file=sys.stderr)

        return score1
        #return 1000 * (len(v[0]) - len(v[1]))#score / (depth + 1) #if maxPlayer else -score

    def __str__(self, **kwargs):
        return "[{}-{},{}]".format(self.state.value, self.x, self.y)
    
def voronoi(graph, player1, player2):
    p1Points = []
    p2Points = []

    p1Dist = player1.distance
    p2Dist = player2.distance
    p1p = p1Points.append
    p2p = p2Points.append
    p1c = player1.checkNeighbor
    p2c = player2.checkNeighbor

    for x in range(30):
        for y in range(20):
            point = graph[x][y]

            p1Access = p1c(x, y)
            p2Access = p2c(x, y)

            if p1Access and not p2Access: p1p(point)
            if p2Access and not p1Access: p2p(point)
            
            d1 = p1Dist(point)
            d2 = p2Dist(point)

            if d1 <= d2:
                p1p(point)
            else:
                p2p(point)

    return (p1Points, p2Points)

def availableNodes(graph, start):
    openSet = deque()
    openSet.append(start)

    closedSet = []
    while len(openSet) > 0:
        current = openSet.popleft()

        for n in current.neighbors(graph):
            if n not in closedSet: closedSet.append(n)
            if n not in openSet and n not in closedSet: openSet.append(n)
            
    #print("dj: {} for {}".format(len(closedSet), start), file=sys.stderr)
    return closedSet

#build the map
graph = []
for w in range(30):
    col = []
    for h in range(20):
        col.append(Node(w, h, State.EMPTY))
    graph.append(col)

def negaMax(node, depth, alpha, beta, maximizingPlayer, graph):
    #print("Score1: {} for {}".format(node, depth), file=sys.stderr)
    if depth == 0 or len(node.neighbors(graph)) == 0:
        return node.score(graph, depth, maximizingPlayer)
       
    for neighbor in node.neighbors(graph):
        temp = neighbor.state
        neighbor.state = State.PLAYER
        v = -negaMax(neighbor, depth - 1, -beta, -alpha, not maximizingPlayer, graph)
        neighbor.state = temp
        #print("V: {} A: {}".format(v, alpha), file=sys.stderr)
        alpha = max(alpha, v)
        if alpha >= beta:
            break

    return alpha
    

def bestMove(player, graph):
    bestVal = -math.inf
    move = player
    
    for neighbor in graph[player.x][player.y].neighbors(graph):
        temp = neighbor.state
        v = negaMax(neighbor, 4, -math.inf, math.inf, True, graph)
        neighbor.state = temp
        
        #print("P: {} N: {} S: {} v {}".format( player,neighbor, v, bestVal), file=sys.stderr)
        #print("Score: {} for {}".format(v, neighbor), file=sys.stderr)
        if v > bestVal:
            bestVal = v
            move = neighbor

            
    #print("Move: {}".format(move), file=sys.stderr)
    return move

players = []
p = 0
# game loop
while True:
    players = []
    # n: total number of players (2 to 4).
    # p: your player number (0 to 3).
    n, p = [int(i) for i in input().split()]
    for i in range(n):
        # x0: starting X coordinate of lightcycle (or -1)
        # y0: starting Y coordinate of lightcycle (or -1)
        # x1: starting X coordinate of lightcycle (can be the same as X0 if you
        # play before this player)
        # y1: starting Y coordinate of lightcycle (can be the same as Y0 if you
        # play before this player)
        x0, y0, x1, y1 = [int(j) for j in input().split()]
        players.append(Player(i, x0, y0, x1, y1))
        graph[x1][y1].state = State.PLAYER

    #for i in range(20):
    #    s = ''
    #    for x in range(30):
    #        s += " {}".format(graph[x][i])
    #    print("{}".format(s), file=sys.stderr)

    move = bestMove(players[p], graph)
    d = players[p].direction(move)
    
    graph[move.x][move.y].state = State.PLAYER

    #print("{} {}".format(move, d), file=sys.stderr)
    
    # To debug: print("Debug messages...", file=sys.stderr)

    # A single line with UP, DOWN, LEFT or RIGHT
    print(d)

    for i in range(n):
        graph[players[i].x][players[i].y].state = State.WALL
