using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* 
 * <#ff9900><b>[activate name activate] : </b><#ffff00>activates/deactivates child game object
<#00ff00>controls activation of child gameobject
<#00eeff><b>name: </b><#0099ff>child gameobject name, string, default to ''
<#00eeff><b>activate: </b><#0099ff>activate or deactivate, true/false, default to false*/

namespace ZeptoBt
{
    public class NodeDecoratorInvert : NodeDecorator
    {
        public override string Documentation { get; } =
            "<#ff9900><b>[invertify] : </b><#ffff00>invert decorator\n" +
            "<#00ff00>inverts the result of the child node";

        public override Doc Doc { get; } = new Doc()
        {
            name = "invertify",
            description = "inverts the result of the child node",
            prototype = "invertify",
            parameters = new DocParameter[] { }
        };
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
        public override string Documentation { get; } =
            "<#ff9900><b>[successify] : </b><#ffff00>force success decorator\n" +
            "<#00ff00>always returns success whether the child execution succeeds or fails";

        public override Doc Doc { get; } = new Doc()
        {
            name = "successify",
            description = "always returns success whether the child execution succeeds or fails",
            prototype = "successify",
            parameters = new DocParameter[] { }
        };

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
        public override string Documentation { get; } =
            "<#ff9900><b>[failify] : </b><#ffff00>force success decorator\n" +
            "<#00ff00>always returns faillure whether the child execution succeeds or fails";

        public override Doc Doc { get; } = new Doc()
        {
            name = "failify",
            description = "always returns faillure whether the child execution succeeds or fails",
            prototype = "failify",
            parameters = new DocParameter[] { }
        };
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
        public override string Documentation { get; } =
            "<#ff9900><b>[onceify] : </b><#ffff00>executes child only once\n" +
            "<#00ff00>executes child once, returns succes once child has executed";

        public override Doc Doc { get; } = new Doc()
        {
            name = "onceify",
            description = "executes child once, returns succes once child has executed",
            prototype = "onceify",
            parameters = new DocParameter[] { }
        };

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

        public override string Documentation { get; } =
            "<#ff9900><b>[gatify] : </b><#ffff00>executes child only once it has returned success\n" +
            "<#00ff00>executes child once, returns success once child has executed with success";

        public override Doc Doc { get; } = new Doc()
        {
            name = "gatify",
            description = "executes child once, returns success once child has executed with success",
            prototype = "gatify",
            parameters = new DocParameter[] { }
        };

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

        public override string Documentation { get; } =
            "<#ff9900><b>[rowresetify] : </b><#ffff00>executes child once until row has reset\n" +
            "<#00ff00>executes child once, resets if a previous node fails or wait";

        public override Doc Doc { get; } = new Doc()
        {
            name = "rowresetify",
            description = "executes child once, resets if a previous node fails or wait",
            prototype = "rowresetify",
            parameters = new DocParameter[] { }
        };

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
        public override string Documentation { get; } =
            "<#ff9900><b>[repeatify] : </b><#ffff00>repeats child if it returns success\n" +
            "<#00ff00>if the child returns success it resets the child";

        public override Doc Doc { get; } = new Doc()
        {
            name = "repeatify",
            description = "if the child returns success it resets the child",
            prototype = "repeatify",
            parameters = new DocParameter[] { }
        };

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

    public class NodeDecoratorThreshold : NodeDecorator
    {
        public override string Documentation { get; } =
            "<#ff9900><b>[thresholdify thresholds] : </b><#ffff00>detects child thresold\n" +
            "<#00ff00>returns success only when the child crosses a threshold\n" +
            "<#00eeff><b>thresholds: </b><#0099ff>three characters that encode thresholds that are detected for success/failure/run. " +
            "Each character can be [T]o, [F]rom, [B]oth, [N]one.\n" +
            "For example 'TFN' will trigger when going to success and when coming from failure.\n" +
            "String variable, defaults to TNN\n";

        public override Doc Doc { get; } = new Doc()
        {
            name = "thresholdify [thresholds]",
            description = "returns success only when the child crosses a threshold\n",
            prototype = "wait [timeout] [mode]",
            parameters = new DocParameter[]
            {
                new DocParameter()
                {
                    name = "thresholds",
                    description = "three characters that encode thresholds that are detected for success/failure/run. " +
                        "Each character can be [T]o, [F]rom, [B]oth, [N]one.\n" +
                        "For example 'TFN' will trigger when going to success and when coming from failure.",
                    isEvaluated = true,
                    defaultValue = "TNN" 
                }
            }
        };


        private NodeParam<string> thresholds = new NodeParam<string>("tnn");
        private NodeReturn previousChildState = NodeReturn.Unprocessed;

        public override string[] Params
        {
            get => base.Params;
            set
            {
                base.Params = value;

                if (base.Params.Length > 0) thresholds.Set(base.Params[0]);
            }
        }

        public override void Tick()
        {
            if (Children.Count != 1)
            {
                Status = NodeReturn.Success;
                return;
            }

            Children[0].Tick();

            if (Children[0].Status == previousChildState)
            {
                Status = NodeReturn.Failure;
                return;
            }

            var th = thresholds.Get(Root.Evaluator);
 
            if (th.Length != 3)
            {
                Status = NodeReturn.Failure;
                return;
            }

            th = th.ToLower();

            switch (Children[0].Status)
            {
                case NodeReturn.Success:
                    if (th[0] == 't' || th[0] == 'b') Status = NodeReturn.Success;
                    if (previousChildState == NodeReturn.Failure && (th[1] == 'f' || th[0] == 'b')) Status = NodeReturn.Success;
                    if (previousChildState == NodeReturn.Runnning && (th[2] == 'f' || th[0] == 'b')) Status = NodeReturn.Success;
                    break;
                case NodeReturn.Failure:
                    if (previousChildState == NodeReturn.Success && (th[0] == 'f' || th[0] == 'b')) Status = NodeReturn.Success;
                    if (th[1] == 't' || th[0] == 'b') Status = NodeReturn.Success;
                    if (previousChildState == NodeReturn.Runnning && (th[2] == 'f' || th[0] == 'b')) Status = NodeReturn.Success;
                    break;
                case NodeReturn.Runnning:
                    if (previousChildState == NodeReturn.Success && (th[0] == 'f' || th[0] == 'b')) Status = NodeReturn.Success;
                    if (previousChildState == NodeReturn.Success && (th[1] == 'f' || th[0] == 'b')) Status = NodeReturn.Success;
                    if (th[2] == 't' || th[0] == 'b') Status = NodeReturn.Success;
                    break;
                default: Status = NodeReturn.Failure; break;
            }

            previousChildState = Children[0].Status;
        }
    }

    public class NodeSequence : NodeComposite
    {
        public override string Documentation { get; } =
            "<#ff9900><b>[>] : </b><#ffff00>executes childs in sequence\n" +
            "<#00ff00>executes child in sequence, whenever a child fails fails\n" +
            "returns wait if a child wait\n" +
            "returns success if all children succeed";

        public override Doc Doc { get; } = new Doc()
        {
            name = ">",
            description = "executes child in sequence, whenever a child fails fails\n" +
                "returns wait if a child wait\n" +
                "returns success if all children succeed",
            prototype = ">",
            parameters = new DocParameter[] { }
        };

        public override void Tick()
        {
            int i = 0;

            if (Root == null)
            {
                Debug.LogError("Root is null");
                return;
            }

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
        public override string Documentation { get; } =
            "<#ff9900><b>[?] : </b><#ffff00>selects first child to succeed\n" +
            "<#00ff00>executes child in sequence, whenever a child fails tries the next\n" +
            "returns wait if a child wait\n" +
            "returns success if a child succeeds";

        public override Doc Doc { get; } = new Doc()
        {
            name = "?",
            description = ">executes child in sequence, whenever a child fails tries the next\n" +
                "returns wait if a child wait\n" +
                "returns success if a child succeeds",
            prototype = "?",
            parameters = new DocParameter[] { }
        };

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
        public override string Documentation { get; } =
            "<#ff9900><b>[wait timeout mode] : </b><#ffff00>waits for a timeout\n" +
            "<#00ff00>returns wait until timeout expires, timeout can be reset with tree abort\n" +
            "<#00eeff><b>timeout: </b><#0099ff>timeout before success, float variable, default to 0\n" +
            "<#00eeff><b>mode: </b><#0099ff>Block timer does not reset / Skip timer is reset at success, Block/Skip variable, default to Block\n" +
            "returns success if a child succeeds";

        public override Doc Doc { get; } = new Doc()
        {
            name = "wait",
            description = "returns wait until timeout expires, timeout can be reset with tree abort",
            prototype = "wait [timeout] [mode]",
            parameters = new DocParameter[]
            {
                new DocParameter() { name = "timeout", description = "timeout before success", isEvaluated = true, defaultValue = "0" },
                new DocParameter() { name = "mode", description = "Block => timer does not reset *OR* Skip => timer is reset at success", isEvaluated = true, defaultValue = "Block" },
            }
        };

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
        public override string Documentation { get; } =
            "<#ff9900><b>[$ ! expression] : </b><#ffff00>expression evaluator\n" +
            "<#00ff00>evaluates expression, variables can be used\n" +
            "<#00eeff><b>!: </b><#0099ff>evaluate once, if ! leads the expresison the expression is only evaluated once\n";

        public override Doc Doc { get; } = new Doc()
        {
            name = "$",
            description = "evaluates expression, variables can be used",
            prototype = "$ [!] [expression]",
            parameters = new DocParameter[]
            {
                new DocParameter() { name = "!", description = "evaluate once, if ! leads the expresison the expression is only evaluated once", isEvaluated = false, defaultValue = "" },
                new DocParameter() { name = "expression", description = "the expression to  be evaluated", isEvaluated = false, defaultValue = "" },
            }
        };

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
            //Debug.Log("EXP " + expression);
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
        public override string Documentation { get; } =
            "<#ff9900><b>[activate name activate] : </b><#ffff00>activates/deactivates child game object\n" +
            "<#00ff00>controls activation of child gameobject\n" +
            "<#00eeff><b>name: </b><#0099ff>child gameobject name, string, default to ''\n" +
            "<#00eeff><b>activate: </b><#0099ff>activate or deactivate, true/false, default to false\n";

        public override Doc Doc { get; } = new Doc()
        {
            name = "activate",
            description = "controls activation of child gameobject",
            prototype = "activate [name] [is_active]",
            parameters = new DocParameter[]
            {
                new DocParameter() { name = "name", description = "child gameobject name", isEvaluated = false, defaultValue = "" },
                new DocParameter() { name = "is_active", description = "boolean that sets the child gameobject active or not (true or false)", isEvaluated = false, defaultValue = "false" },
            }
        };

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
