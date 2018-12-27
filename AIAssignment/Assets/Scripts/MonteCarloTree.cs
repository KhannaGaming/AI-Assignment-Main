using System;
using System.Collections.Generic;

namespace MonteCarloTree
{
    enum NodeOptions { Nothing, dontHaveEnemyFlag, haveEnemyFlag, HealthKit, PowerUp, RandomMovement, FriendlyFlag };

    public class Node
    {
        private NodeOptions thisNodeOption = NodeOptions.Nothing;
        public int NodeOption
        {
            get { return (int)thisNodeOption; }
            set { thisNodeOption = (NodeOptions)value; }
        }

        private Node parentNode;
        public Node Parent
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        private List<Node> childNodes;
        
        private int visitCount = 0;
        public int VisitCount
        {
            get { return visitCount; }
            set { visitCount = value; }
        }

        private int winCount = 0;
        public int WinCount
        {
            get { return winCount; }
            set { winCount = value; }
        }

        public Node()
        {
            childNodes = new List<Node>();
        }

        public void AddChild(Node newChild)
        {
            newChild.Parent = this;
            childNodes.Add(newChild);
        }
        public bool checkChildren(int nodeOption)
        {
            for (int i = 0; i < childNodes.Count; i++)
            {
                if(childNodes[i].NodeOption == nodeOption)
                {
                    return true;
                }
            }
            return false;
        }
        public Node ParentNode
        {
            get { return parentNode; }
        }

        public int GetNumberOfChildren()
        {
            return childNodes.Count;
        }

        public Node GetChild(int index)
        {
            return childNodes[index];            
        }
        public Node GetRandomChild()
        {
            if (childNodes.Count > 0)
            {
                return childNodes[new Random().Next(0, childNodes.Count - 1)];
            }

            return null;
        }
    }

    public class Tree
    {
        private Node rootNode;
        private Node currentNode;
        public Tree()
        {
            rootNode = new Node();
            currentNode = rootNode;
        }

        public void Traverse(Node currentNode)
        {
            int numberOfChildNodes = currentNode.GetNumberOfChildren();

            if (numberOfChildNodes > 0)
            {
                for (int i = 0; i < numberOfChildNodes; i++)
                {
                    Node childToTraverse = currentNode.GetChild(i);
                        Traverse(childToTraverse);
                }
            }
        }

    }

}
