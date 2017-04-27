import Glibc

public struct StderrOutputStream: OutputStreamType {
    public mutating func write(string: String) { fputs(string, stderr) }
}
public var errStream = StderrOutputStream()

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

let N = Int(readLine()!)!
if N > 0 {
    for i in 0...(N-1) {
        let inputs = (readLine()!).characters.split{$0 == " "}.map(String.init)
        let X = Int(inputs[0])!
        let Y = Int(inputs[1])!
    }
}

// Write an action using print("message...")
// To debug: debugPrint("Debug messages...", toStream: &errStream)

print("answer")