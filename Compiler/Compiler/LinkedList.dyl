//Linked List

namespace MyNamespace
{
    class[public] Node
    {
        var[public] int Value;
        var[public] Node Next;
        method[public] Node(var int Value)
        {
            this.Value = Value;
        }
    }

    class[public] LinkedList
    {
        var[public] Node Head;
        var[public] Node Tail;
        var[public] int Count;
        method[public] LinkedList()
        {
            Count = 0;
        }

        method[public] void AddFirst(var int Value)
        {
            Count++;
            if(Head == null)
            {
                Head = new Node(Value);
                Head.Next = Head;
                Tail = Head;
                return;
            }
            var Node temp = new Node(Value);
            temp.Next = Head;
            Tail.Next = temp;
            Head = Tail.Next;
        }
        method[public] void AddLast(var int Value)
        {
            Count++;
            if(Head == null)
            {
                Head = new Node(Value);
                Head.Next = Head;
                Tail = Head;
                return;
            }
            var Node temp = new Node(Value);
            temp.Next = Head;
            Tail.Next = temp;
            Tail = Tail.Next;
        }
        method[public] bool Contains(var int Value)
        {
            var Node currentNode = Head;
            int i = 0;
            while(i<Count)
            {
                if(curretNode.Value == Value)
                {
                    return true;
                }
                currentNode = currentNode.Next;
                i++;
            }
            return false;
        }
    }

    class[public] Program
    {
        method[public] entrypoint void Main()
        {
            var LinkedList list = new LinkedList();
            list.AddFirst(5);
            var bool contains = list.Contains(5);
        }
    }
}