using Language;
using Language.Tokens;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Parallel.Tree
{
    public class Tree : ICloneable
    {
        #region Private fields

        private Node root;
        private List<Node> rotated;

        #endregion

        #region Private methods

        private Tree(Node root)
        {
            this.root = root;
            this.rotated = new List<Node>();
        }

        private Node CloneNode(Node original, Node newParent)
        {
            Node result = new Node(newParent, original.Value);

            foreach (var child in original.Children)
                result.Children.Add(CloneNode(child, result));

            return result;
        }
        
        private int SubtreeHeight(Node root)
        {
            /* Recursion exit condition */
            if (root.Children.Count == 0)
                return 0;

            /* The height of current tree is
             * the maximum height of the child subtrees + 1 */
            return root.Children.Select(child => SubtreeHeight(child)).Max() + 1;
        }

        private IEnumerable<Node> TraverseInOrder(Node current)
        {
            yield return current;

            foreach (var child in current.Children)
                foreach (var childChild in TraverseInOrder(child))
                    yield return childChild;
        }

        private bool WorthRotateLeft(Node current, out int heightGain)
        {
            int rH = SubtreeHeight(current.Children.Last());
            int llH = SubtreeHeight(current.Children.First().Children.First());
            int lrH = SubtreeHeight(current.Children.First().Children.Last());

            int currentHeight = Math.Max(rH, Math.Max(llH, lrH) + 1) + 1;
            int newHeight = Math.Max(llH, Math.Max(lrH, rH) + 1) + 1;

            heightGain = newHeight - currentHeight;
            return heightGain < 0;
        }

        private bool WorthRotateRight(Node current, out int heightGain)
        {
            int lH = SubtreeHeight(current.Children.First());
            int rrH = SubtreeHeight(current.Children.Last().Children.Last());
            int rlH = SubtreeHeight(current.Children.Last().Children.First());

            int currentHeight = Math.Max(lH, Math.Max(rrH, rlH) + 1) + 1;
            int newHeight = Math.Max(rrH, Math.Max(rlH, lH) + 1) + 1;

            heightGain = newHeight - currentHeight;
            return heightGain < 0;
        }

        private bool CanRotateLeft(Node current)
        {
            /* Skip unary operators and leaves' parents */
            if (current.Children.Count < 2)
                return false;

            string op = current.Value;
            Node leftChild = current.Children.First();

            /* Check if current node and its left child
             * contain the same operator or a pair of inversed operators */
            if (Utils.IsOperator(op) &&
                (leftChild.Children.Count > 1) &&       // omit unary child operators
                (leftChild.Value == op || leftChild.Value == Utils.GetInverse(op)))
            {
                return true;
            }
            return false;
        }

        private bool CanRotateRight(Node current)
        {
            /* Skip leaves' parent operators */
            if (current.Children.Count == 0)
                return false;

            string op = current.Value;
            Node rightChild = current.Children.Last();

            /* Check if current node and its right child
             * contain the same operator or a pair of inversed operators */
            if (Utils.IsOperator(op) &&
                (rightChild.Value == op || rightChild.Value == Utils.GetInverse(op)))
            {
                return true;
            }
            return false;
        }

        private void RotateLeft(Node current)
        {
            Node parent = current.Parent;
            Node leftChild = current.Children.First();

            Node leftChildRight = leftChild.Children.Last();
            int currentIndex = -1;

            /* Remove 3 edges */
            leftChild.Children.RemoveAt(1);
            current.Children.RemoveAt(0);

            if (parent != null)
            {
                currentIndex = parent.Children.IndexOf(current);
                parent.Children.RemoveAt(currentIndex);
            }

            /* Invert current node operation for '-' and '/' */
            if ((current.Value == "-" || current.Value == "/") &&
                leftChild.Value == current.Value)
            {
                current.Value = Utils.GetInverse(current.Value);
            }

            /* Add 3 edges */
            leftChild.Parent = parent;
            leftChild.Children.Add(current);

            current.Parent = leftChild;
            current.Children.Insert(0, leftChildRight);

            leftChildRight.Parent = current;

            /* Update parent child node */
            if (parent == null)
                this.root = leftChild;
            else
                parent.Children.Insert(currentIndex, leftChild);
        }

        private void RotateRight(Node current)
        {
            Node parent = current.Parent;
            Node rightChild = current.Children.Last();

            Node rightChildLeft = rightChild.Children.First();
            int currentIndex = -1;

            /* Remove 3 edges */
            rightChild.Children.RemoveAt(0);
            current.Children.RemoveAt(1);

            if (parent != null)
            {
                currentIndex = parent.Children.IndexOf(current);
                parent.Children.RemoveAt(currentIndex);
            }

            /* Add 3 edges */
            rightChild.Parent = parent;
            rightChild.Children.Insert(0, current);

            current.Parent = rightChild;
            current.Children.Add(rightChildLeft);

            rightChildLeft.Parent = current;

            /* Update parent child node */
            if (parent == null)
                this.root = rightChild;
            else
                parent.Children.Insert(currentIndex, rightChild);
        }

        private void Rotate(Node current)
        {
            Node child = current.Children.FirstOrDefault(node => !rotated.Contains(node));
            while (child != null)
            {
                Rotate(child);
                child = current.Children.FirstOrDefault(node => !rotated.Contains(node));
            }

            /* Transform a - -b into a + b */
            if (current.Value == "-" &&
                current.Children.Count > 0)
            {
                Node right = current.Children.Last();
                
                if (right.Value == "-" &&
                    right.Children.Count == 1)
                {
                    current.Value = "+";
                    current.Children.Remove(right);
                    current.Children.Add(right.Children.First());
                    current.Children.First().Parent = current;
                }
            }

            int rightGain = 0;
            int leftGain = 0;
            int choice = 0;

            if (CanRotateLeft(current) && WorthRotateLeft(current, out leftGain))
                choice = 1;
            else if (CanRotateRight(current) && WorthRotateRight(current, out rightGain))
                choice += 2;

            switch (choice)
            {
                /* Rotation can't/isn't worth to be performed */
                case 0:
                    break;

                /* Both left and right rotation can be performed */
                case 3:
                    if (leftGain < rightGain)
                        RotateLeft(current);
                    else
                        RotateRight(current);
                    break;

                case 1:
                    RotateLeft(current);
                    break;

                case 2:
                    RotateRight(current);
                    break;
            }

            rotated.Add(current);
        }

        #endregion

        #region Public properties

        public int Height
        {
            get { return SubtreeHeight(this.root); }
        }

        public IEnumerable<Node> Nodes
        {
            get { return TraverseInOrder(root); }
        }

        public IEnumerable<Arc> Arcs
        {
            get { return root.OutArcs; }
        }

        public List<Node> LeaveOperations
        {
            get { return this.Nodes.Where(node => node.Children.Count > 0 &&
                                                  node.Children.Sum(child => child.Children.Count) == 0)
                                   .ToList(); }
        }

        #endregion

        #region Public methods

        public Node FindNode(IEnumerable<int> path)
        {
            Node current = this.root;
            foreach (int childIndex in path)
            {
                current = current.Children[childIndex];
            }
            return current;
        }

        /// <summary>
        /// Tries to reduce the height of the tree
        /// </summary>
        public void Balance()
        {
            rotated.Clear();
            Rotate(this.root);
        }

        public object Clone()
        {
            return new Tree(CloneNode(root, null));
        }

        #endregion

        #region Static methods

        private static Node OperandToNode(Node parent, string operand)
        {
            if (operand[0] == '-')
            {
                string abs = operand.Substring(1);
                double constant;

                if (double.TryParse(abs, out constant))
                    return new Node(parent, operand);

                Node minus = new Node(parent, "-");
                minus.Children.Add(new Node(minus, abs));

                return minus;
            }
            return new Node(parent, operand);
        }

        private static void ExpandUnary(Node current)
        {
            if (current.Children.Count == 0)
            {
                if (current.Value != "-" && current.Value[0] == '-')
                {
                    double number;
                    if (!double.TryParse(current.Value,
                                         NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                                         NumberFormatInfo.InvariantInfo, out number))
                    {
                        current.Children.Add(new Node(current, current.Value.Substring(1)));
                        current.Value = "-";
                    }
                }
            }
            else
            {
                foreach (var child in current.Children)
                    ExpandUnary(child);
            }
        }

        public static Tree FromTokens(IList<Token> tokens)
        {
            if (tokens.Count == 0)
                return new Tree(null);

            /* Convert tokens into a prefix expression notation */
            var ops = Utils.TokensToOps(tokens);
            ops = Utils.InfixToPostfix(ops);

            /* Convert postfix expression into the binary tree */            
            Node root = new Node(null, ops[ops.Count - 1]);
            Node current = root;

            for (int k = ops.Count - 2; k >= 0; k--)
            {
                /* Add current operator/operand as a child node of the current one */
                string nodeValue = ops[k];
                current.Children.Insert(0, new Node(current, nodeValue));

                /* Move downwards if an operator (the inner node) was added */
                if (Utils.IsOperator(nodeValue))
                    current = current.Children.First();

                /* Move upwards if the last possible operand (leaf node) was added */
                else
                {
                    while (current.Children.Count == 2 && current.Parent != null)
                        current = current.Parent;
                }
            }

            ExpandUnary(root);
            return new Tree(root);
        }

        #endregion
    }
}
