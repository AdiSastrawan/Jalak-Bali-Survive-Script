using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace AdiSastrawan.Node
{
    public abstract class Node 
    {
        public string name;
        public List<Node> children = new();
        protected int currentChild;
        public Node(string name = "") 
        {
            this.name = name;
        }
        public void AddChild(Node child)
        {
            children.Add(child);
        }
        public virtual Status Process()=>children[currentChild].Process();
        public virtual void Reset()
        {
            currentChild = 0;
            foreach(Node child in children)
            {
                child.Reset();
            }
        }
    }
    public class Leaf : Node
    {
        public IStrategy strategy;
        public Leaf(string name,IStrategy strategy) : base(name)
        {
            this.strategy = strategy;
        }
        public override Status Process(){
            Status status = strategy.Process();
            //Debug.Log("Processing ... " + name + $", {status}");
            return status;
        }
        public override void Reset() => strategy.Reset();

    }
    public class Inverter : Node
    {
        public override Status Process()
        {
            Status status = children[0].Process();
            if(status == Status.Running)
                return status;
            return status == Status.Success ? Status.Failed:Status.Success;
        }
    }
    public class BehaviourTree : Node
    {
        public BehaviourTree(string name) : base(name)
        {
        }
        public override Status Process()
        {

            while(currentChild < children.Count)
            {
                Status status = children[currentChild].Process();
                if (status!= Status.Success)
                {
                    return status; 
                }
                currentChild++;
            }
            if(currentChild >= children.Count) currentChild = 0;
            return Status.Success;
        }
    }

    public class Sequence : Node
    {
        public Sequence(string name) : base(name) { }

        public override Status Process()
        {
           if(currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    
                    case Status.Failed:
                        Reset();
                        return Status.Failed;
                    case Status.Running:
                        return Status.Running;
                    default:
                        currentChild++;
                        return currentChild == children.Count ? Status.Success : Status.Running;
                }
            }
            Reset();
            return Status.Success;
        }
    }

    public class Selector : Node
    {
        public Selector(string name) : base(name) { }

        public override Status Process()
        {
            if(currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {

                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        Debug.Log("Move to the next child");
                        currentChild++;
                        return Status.Running;
                }
            }
            Reset();
            return Status.Failed;
        }
    }

    public class UntilFail : Node
    {
        public UntilFail(string name) : base(name) { }
        public override Status Process()
        {

            if(currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Failed:
                        Reset();
                        return Status.Failed;
                    case Status.Running:
                        return Status.Running;
                    default:
                        currentChild++;
                        if (currentChild ==  children.Count) currentChild = 0;
                        return Status.Running;

                }
            }
            return Status.Running;
        }
    }
    public class UntilSuccess : Node
    {
        public override Status Process()
        {
            if(currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    case Status.Running:
                        return Status.Running;
                    default:
                        currentChild++;
                        if (currentChild == children.Count) currentChild = 0;
                        return Status.Running;
                }
            }
            return Status.Running;
        }
    }
    public enum Status
    {
        Running,
        Success,
        Failed
    }

}
