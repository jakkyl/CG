/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
function Node(person, influence)
{
    this.person = person;
    this.parent = influence;
    this.children = [];
}

function Tree(person)
{
    this.root = new Node(person);
}

Tree.prototype.traverse = function(callback)
{
    (function recurse(current)
    {
        for (var i = 0; i < current.children.length; i++)
        {
            recurse(current.children[i]);
        }

        callback(current);
    })(this.root);
};

Tree.prototype.contains = function(callback, traversal)
{
    this.traverse(callback);
};

Tree.prototype.add = function(person, influence)
{
    var parent;
    (function recurse(current)
    {
        if (current.person == influence)
        {
            parent = current;
            return;
        }

        for (var i = 0; i < current.children.length; i++)
        {
            recurse(current.children[i]);
        }
    })(this.root);

    if (parent)
    {
        //printErr('Adding child: ' + person + ' to ' + parent);
        var child = new Node(person, influence);

        parent.children.push(child);
    }
}

var treeOfInfl = null;

var n = parseInt(readline()); // the number of relationships of influence
printErr('n: ' + n);
for (var i = 0; i < n; i++)
{
    var inputs = readline().split(' ');
    var x = parseInt(inputs[0]); // a relationship of influence between two people (x influences y)
    var y = parseInt(inputs[1]);
    printErr('Rel: ' + x + ' ' + y);
    if (!treeOfInfl)
    {
        treeOfInfl = new Tree(x);
    }

    treeOfInfl.add(y, x);
}

treeOfInfl.traverse(function(node) { printErr(node.person); })
// Write an action using print()
// To debug: printErr('Debug messages...');

// The number of people involved in the longest succession of influences
print('2');