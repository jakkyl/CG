import Glibc

public struct StderrOutputStream: OutputStreamType {
    public mutating func write(string: String) { fputs(string, stderr) }
}
public var errStream = StderrOutputStream()

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

let inputs = (readLine()!).characters.split{$0 == " "}.map(String.init)
let W = Int(inputs[0])! // number of columns.
let H = Int(inputs[1])! // number of rows.
var graph = Array(count: W, repeatedValue:Array(count: H, repeatedValue: Int()))

if H > 0 {
    for i in 0...(H-1) {
        let LINE = (readLine()!).characters.split(" ").map(String.init) // represents a line in the grid and contains W integers. Each integer represents one room of a given type.
        debugPrint(LINE, toStream: &errStream)
        for j in 0...(W-1) {
            graph[j][i] = Int(LINE[j])!
        }
    }
}
let EX = Int(readLine()!)! // the coordinate along the X axis of the exit (not useful for this first mission, but must be read).
var lastPos = "TOP"


func  getX(pos: String, tileType: Int, last: String) -> Int
{
    switch (tileType)
    {
        case 2, 6:
            if pos == "LEFT"
            {
                return 1
            }
            else
            {
                return -1
            }

        case 5, 11:
            if pos == "TOP"
            {
                return 1
            }
        case 4, 10:
            if pos == "TOP"
            {
                return -1
            }
        default:
            return 0
    }
        
    return 0    
}

func  getY(pos: String, tileType: Int, last: String) -> Int
{
    switch (tileType)
    {
        case 1, 3, 7, 8, 9:            
            return 1            
        case 5, 13:
            if pos == "LEFT"
            {
                return 1
            }
        case 4, 12:
            if pos == "RIGHT"
            {
                return 1
            }
        default:
            return 0
    }
        
    return 0    
}

// game loop
while true {
    let inputs = (readLine()!).characters.split{$0 == " "}.map(String.init)
    let XI = Int(inputs[0])!
    let YI = Int(inputs[1])!
    let POS = inputs[2]
    debugPrint(POS, toStream: &errStream)
    var x = XI + getX(POS, tileType: graph[XI][YI], last: lastPos)
    var y = YI + getY(POS, tileType: graph[XI][YI], last: lastPos)
    debugPrint("\(graph[XI][YI])", toStream: &errStream)

    // Write an action using print("message...")
    // To debug: debugPrint("Debug messages...", toStream: &errStream)
    lastPos=POS

    // One line containing the X Y coordinates of the room in which you believe Indy will be on the next turn.    
    print("\(x) \(y)")
}
