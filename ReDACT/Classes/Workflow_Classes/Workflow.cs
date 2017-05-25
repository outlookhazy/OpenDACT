using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.Workflow_Classes.Workflow;

namespace OpenDACT.Class_Files.Workflow_Classes
{
     public class Workflow
    {
        protected event WorkflowStateChangeHandler OnEvent;
        private LinkedList<Workflow> WorkflowQueue = new LinkedList<Workflow>();
        private LinkedListNode<Workflow> WorkflowItem;
        private WorkflowStateChangeHandler _parent;
        private WorkflowState status = WorkflowState.PENDING;
        public WorkflowState Status { get { return status; } private set { status = value; } }
        public string ID { get; set; }

        public void AddWorkflowItem(Workflow item)
        {
            this.DebugState("added workflow item.");
            this.WorkflowQueue.AddLast(item);
        }

        public void Start()
        {            
            this.Start(FakeParent);
        }

        private void FakeParent(object sender, WorkflowState newState)
        {

        }

        public void Start(WorkflowStateChangeHandler parent)
        {
            this.DebugState("Start begun");
            this._parent = parent;
            this.OnEvent += parent;
            this.UpdateStatus(WorkflowState.PENDING);
            this.OnStarted();
            this.UpdateStatus(WorkflowState.STARTED);
            this.DebugState("Start complete");
        }

        private void Workflow_OnChildStateChange(object sender, WorkflowState newState)
        {
            switch (newState)
            {
                case WorkflowState.FINISHED:
                    this.DebugState("Child finished");
                    this.FinishOrAdvance();
                    break;
                case WorkflowState.ABORTED:
                    this.DebugState("Child aborted");
                    this.Abort();
                    break;
            }
        }

        protected virtual void OnStarted()
        {
            this.ID = "DefaultWF";
            this.DebugState("Default OnStarted called");
        }

        protected virtual void OnMessage(string serialMessage)
        {
            this.DebugState("Default OnMessage called");
            this.FinishOrAdvance();
        }

        protected virtual void OnAborted()
        {
            this.DebugState("Default OnAborted called");

        }

        protected virtual void OnChildrenFinished()
        {
            this.DebugState("Default OnChildrenFinished called");
        }

        public void RouteMessage(string serialMessage)
        {
            this.DebugState("Routing message");
            switch (this.Status)
            {
                case WorkflowState.STARTED:
                    this.DebugState("\tTo Self");
                    this.OnMessage(serialMessage);
                    break;
                case WorkflowState.PARTIAL:                    
                    if (this.WorkflowItem != null)
                    { //otherwise pass to current workflow item for routing.
                        this.DebugState("\tTo Child");
                        this.WorkflowItem.Value.RouteMessage(serialMessage);
                    } else
                        this.DebugState("\tTo Nowhere (null child)");
                    break;
                default:
                    this.DebugState("\tTo Nowhere");
                    break;
            }
        }

        protected void UpdateStatus(WorkflowState newStatus)
        {
            this.DebugState("Status updated to " + newStatus);
            this.Status = newStatus;
            this.SendEvent(newStatus);
        }

        public void Abort()
        {
            this.DebugState("Aborting Self");
            this.OnAborted();
            this.DebugState("Aborted Self");
            this.UpdateStatus(WorkflowState.ABORTED);

            if(this.WorkflowItem != null)
            {
                this.DebugState("Aborting Child");
                this.WorkflowItem.Value.Abort();
                this.DebugState("Child Aborted");
                LinkedListNode<Workflow> abortTarget = this.WorkflowItem.Next;
                while(abortTarget != null)
                {
                    this.DebugState("Aborting next Child");
                    abortTarget.Value.Abort();
                    this.DebugState("Next child Aborted");
                    abortTarget = abortTarget.Next;
                }
            }
        }

        protected void FinishOrAdvance()
        {
            this.DebugState("Finishing");
            if (this.Status == WorkflowState.STARTED)
                this.UpdateStatus(WorkflowState.PARTIAL);

            bool complete = false;

            if (this.WorkflowQueue.Count > 0) //queue isn't empty
            {
                if (this.WorkflowItem == null) //first item hasn't been activated
                {
                    this.DebugState("Activating first child");
                    this.WorkflowItem = WorkflowQueue.First;
                    this.WorkflowItem.Value.Start(this.Workflow_OnChildStateChange);
                }
                else //first item has been activated
                {                    
                    LinkedListNode<Workflow> finished = this.WorkflowItem; //save current for removal
                    
                    if (this.WorkflowItem.Next != null) //queue has a next item
                    {                        
                        this.DebugState("Activating next Child");
                        this.WorkflowItem = this.WorkflowItem.Next;                        
                        this.WorkflowItem.Value.Start(this.Workflow_OnChildStateChange);
                    }
                    else
                    {
                        this.WorkflowItem = null; //dispose of the current item
                        this.DebugState("Children are done");
                        complete = true;
                    }
                    WorkflowQueue.Remove(finished);
                }
            }
            else
            {
                this.DebugState("No Children to activate");
                complete = true;
            }

            if(complete)
            {
                this.OnChildrenFinished();
                this.UpdateStatus(WorkflowState.FINISHED);
                this.OnEvent -= _parent;
            }
        }

        protected void SendEvent(WorkflowState newEvent)
        {
            this.OnEvent?.Invoke(this, newEvent);
        }

        private void DebugState(string logmessage)
        {
            string debugtag = this.ID == null ? "<anonymous>" : this.ID; 
            //Debug.WriteLine(String.Format("(WDBG {0}): {1}", debugtag, logmessage));
        }
    }

    

    public enum WorkflowState
    {
        PENDING,
        STARTED,
        PARTIAL,
        FINISHED,
        ABORTED
    }

    public delegate void WorkflowStateChangeHandler(object sender, WorkflowState newState);
}
