using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZeptoBt
{
    public class NodeDecoratorInvert : NodeDecorator
    {
        public override void Tick()
        {
            if (Children.Count != 1)
            {
                Status = NodeReturn.Success;
                return;
            }
            Children[0].Tick();
            NodeReturn returnValue = Children[0].Status;

            switch (returnValue)
            {
                case NodeReturn.Failure: Status = NodeReturn.Success; break;
                case NodeReturn.Success: Status = NodeReturn.Failure; break;
                default: Status = NodeReturn.Runnning; break;
            }
        }
    }

    public class NodeDecoratorSuccessify : NodeDecorator
    {
        public override void Tick()
        {
            if (Children.Count != 1)
            {
                Status = NodeReturn.Success;
                return;
            }

            Children[0].Tick();
            Status = NodeReturn.Success;
        }
    }

    public class NodeDecoratorFailify : NodeDecorator
    {
        public override void Tick()
        {
            if (Children.Count != 1)
            {
                Status = NodeReturn.Failure;
                return;
            }
            Children[0].Tick();
            Status = NodeReturn.Failure;
        }
    }
    public class NodeDecoratorOnce : NodeDecorator
    {
        private bool done;

        public override void Tick()
        {
            if (done)
            {
                Status = NodeReturn.Success;
                return;
            }
            done = true;
            if (Children.Count != 1)
            {
                Status = NodeReturn.Success;
                return;
            }
            Children[0].Tick();
            Status = Children[0].Status;
        }
    }

    public class NodeDecoratorGate : NodeDecorator
    {
        private bool done;
        private bool initDone;

        protected override void OnExit(NodeReturn exitEvent)
        {
            done = false;
        }
        public override void Tick()
        {
            if (done)
            {
                Status = NodeReturn.Success;
                return;
            }

            if (Children.Count != 1)
            {
                Status = NodeReturn.Success;
                return;
            }

            if (!initDone) { compositeParent.ExitEvent += OnExit; initDone = true; }

            Children[0].Tick();
            NodeReturn nodeReturn = Children[0].Status;
            if (nodeReturn == NodeReturn.Success) done = true;
        }
    }

    public class NodeDecoratorRowReset : NodeDecorator
    {
        private bool done;
        private bool initDone;

        protected override void OnExit(NodeReturn exitEvent)
        {
            if (Root.CurrentNode.Index < Index)
                done = false;
        }
        public override void Tick()
        {
            if (done)
            {
                Status = NodeReturn.Success;
                return;
            }

            if (Children.Count != 1)
            {
                Status = NodeReturn.Success;
                return;
            }

            if (!initDone) { Root.ExitEvent += OnExit; initDone = true; }

            Children[0].Tick();
            NodeReturn nodeReturn = Children[0].Status;
            if (nodeReturn == NodeReturn.Success) done = true;
            Status = nodeReturn;
        }
    }

    public class NodeDecoratorRepeat : NodeDecorator
    {
        private bool done;
        private bool initDone;

        public override void Tick()
        {
            if (Children.Count != 1)
            {
                Status = NodeReturn.Success;
                return;
            }

            Status = NodeReturn.Runnning;

            Children[0].Tick();
            NodeReturn nodeReturn = Children[0].Status;
            if (nodeReturn == NodeReturn.Success)
            {
                Children[0].Abort(0);
            }
        }
    }

    public class NodeSequence : NodeComposite
    {
        public override void Tick()
        {
            int i = 0;
            // Debug.Log($"BT TICK - {this}");
            while (i < children.Count)
            {
                Children[i].Tick();
                var childReturn = Children[i].Status;
                if (childReturn == NodeReturn.Runnning)
                {
                    Root.CurrentNode = Children[i];
                    Status = childReturn;
                    return;
                }

                if (childReturn == NodeReturn.Failure) OnExit(NodeReturn.Failure);
                if (childReturn != NodeReturn.Success)
                {
                    Status = childReturn;
                    return;
                }
                i++;
            }

            OnExit(NodeReturn.Success);
            Root.CurrentNode = this;
            Status = NodeReturn.Success;
        }
        public override void Abort(int abortIndex)
        {
            if (abortIndex < Index)
            {
                Children.ForEach(child => child.Abort(Index));
                ChildIndex = 0;
            }
            else
            {
                Children.ForEach(child =>
                {
                    if (child.Index < Index || child is NodeComposite)
                        child.Abort(Index);
                });
            }
        }

        public override string ToString()
        {
            return $"NODE SEQ {Index} {Children.Count}";
        }
    }
    public class NodeSelector : NodeComposite
    {

        public override void Tick()
        {
            int i = 0;
            // Debug.Log($"BT TICK - {this}");
            while (i < Children.Count)
            {
                Children[i].Tick();
                var childReturn = Children[i].Status;
                //if(childReturn == NodeReturn.Runnning && Children[ChildIndex].Index < Tree.CurrentNode.Index)
                //    Tree.Abort(Children[ChildIndex].Index + 1);
                if (childReturn == NodeReturn.Success || childReturn == NodeReturn.Runnning)
                {
                    Root.CurrentNode = Children[i];
                    if (childReturn == NodeReturn.Success) OnExit(NodeReturn.Success);
                    Status = childReturn;
                    return;
                }
                i++;
            }
            Root.CurrentNode = this;
            OnExit(NodeReturn.Failure);
            Status = NodeReturn.Failure;
        }

        public override void Abort(int abortIndex)
        {
            if (abortIndex < Index)
            {
                Children.ForEach(child => child.Abort(Index));
            }
            else
            {
                Children.ForEach(child =>
                {
                    if (child.Index < Index || child is NodeComposite)
                        child.Abort(Index);
                });
            }
        }

        public override string ToString()
        {
            return $"NODE SELECTOR {Index} {Children.Count}";
        }
    }

    public class NodeLeafWait : NodeLeaf
    {
        enum Mode { Block, Skip }

        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;
                if (base.Params.Length > 0) dd.Set(base.Params[0]);
                if (base.Params.Length > 1) mm.Set(base.Params[1]);
            }
        }


        NodeParam<float> dd = new NodeParam<float>();
        NodeParam<Mode> mm = new NodeParam<Mode>();
        enum WaitStatus { Idle, Running, Done }
        WaitStatus waitStatus;
        private float stopDate;

        public override void Abort(int index)
        {
            Debug.Log($"BT ABORT - {this}");
            waitStatus = WaitStatus.Idle;
        }

        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");

            // float localDelay = delayVar == null ? delay: (float)Root.Evaluator.Variables[delayVar];

            switch (waitStatus)
            {
                case WaitStatus.Idle:
                    {
                        Debug.Log("dd " + dd + " Root " + Root + "Tree " + Tree);
                        Debug.Log("dd Root " + Root);
                        stopDate = Tree.CurrentTime + dd.Get(Root.Evaluator);
                        waitStatus = WaitStatus.Running;
                        Status = (mm.Get(Root.Evaluator) == Mode.Block) ? NodeReturn.Runnning : NodeReturn.Runnning;
                        return;
                    }
                case WaitStatus.Running:
                    if (mm.Get(Root.Evaluator) == Mode.Block)
                    {
                        if (Tree.CurrentTime > stopDate)
                        {
                            waitStatus = WaitStatus.Done;
                            Status = NodeReturn.Success;
                            return;
                        }
                        else
                        {
                            Status = NodeReturn.Runnning;
                            return;
                        }
                    }
                    else
                    {
                        if (Tree.CurrentTime > stopDate)
                        {
                            waitStatus = WaitStatus.Idle;
                            Status = NodeReturn.Success;
                            return;
                        }
                        else
                        {
                            Status = NodeReturn.Runnning;
                            return;
                        }
                    }
                case WaitStatus.Done:
                    {
                        Status = NodeReturn.Success;
                        return;
                    }
            }
            Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF WAIT {Index} {waitStatus} {stopDate}";
        }
    }

    public class NodeLeafExpression : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                List<string> localParams = base.Params.ToList();

                if (localParams[0] == "!")
                {
                    onlyOnce = true;
                    localParams.RemoveAt(0);
                }

                expression = localParams.Aggregate("", (a, v) => $"{a} {v}");
            }
        }

        private string expression;
        private bool onlyOnce;
        private bool onlyOnceDone;


        public override void Abort()
        {
            onlyOnceDone = false;
        }
        public override void Tick()
        {
            if (onlyOnceDone)
            {
                Status = NodeReturn.Success;
                return;
            }
            if (onlyOnce) onlyOnceDone = true;

            /// if(Root.Evaluator.Variables.ContainsKey("zzz"))
            /// Debug.Log($"BT EVAL before zzz={Root.Evaluator.Variables["zzz"]}");
            /// 
            Debug.Log("EXP " + expression);
            var result = Root.Evaluator.Evaluate(expression);

            // Debug.Log($"BT TICK - {this} result={result}");
            // Debug.Log($"BT EVAL after zzz={Root.Evaluator.Variables["zzz"]}");

            if (result.GetType() == typeof(bool))
                Status = (bool)result ? NodeReturn.Success : NodeReturn.Failure;
            else
                Status = NodeReturn.Success;
        }

        public override string ToString()
        {
            return $"NODE LEAF EXPRESSION {Index} {expression}";
        }
    }

    public class NodeLeafActivate : NodeLeaf
    {
        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0)
                {
                    goName = base.Params[0];
                }

                if (base.Params.Length > 1)
                {
                    bool.TryParse(base.Params[1], out doActivate);
                }
            }
        }

        string goName;
        bool doActivate;

        public override void Tick()
        {
            // Debug.Log($"BT TICK - {this}");


            if (Tree.Children.ContainsKey(goName))
            {
                Tree.Children[goName].gameObject.SetActive(doActivate);
                Status = NodeReturn.Success;
            }
            else
                Status = NodeReturn.Failure;
        }

        public override string ToString()
        {
            return $"NODE LEAF HIT {Index} {goName} {doActivate}";
        }
    }
}
