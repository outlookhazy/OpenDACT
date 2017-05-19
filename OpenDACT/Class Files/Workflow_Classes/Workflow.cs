using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenDACT.Class_Files.Workflow_Classes.Workflow;

namespace OpenDACT.Class_Files.Workflow_Classes
{
     public abstract class Workflow
    {
        private event WorkflowStateChangeHandler OnEvent;
        private LinkedList<Workflow> WorkflowQueue = new LinkedList<Workflow>();
        private LinkedListNode<Workflow> WorkflowItem;
        private WorkflowStateChangeHandler _parent;
        private WorkflowState status = WorkflowState.PENDING;
        public WorkflowState Status { get { return status; } private set { status = value; } }

        public void AddWorkflowItem(Workflow item)
        {
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
            this._parent = parent;
            this.OnEvent += parent;
            this.UpdateStatus(WorkflowState.PENDING);
            this.OnStarted();
            this.UpdateStatus(WorkflowState.STARTED);
        }

        private void Workflow_onStateChange(object sender, WorkflowState newState)
        {
            switch (newState)
            {
                case WorkflowState.FINISHED:
                    this.FinishOrAdvance();
                    break;
                case WorkflowState.ABORTED:
                    this.Abort();
                    break;
            }
        }

        protected virtual void OnStarted()
        {
            this.FinishOrAdvance();
        }

        protected virtual void OnMessage(string serialMessage)
        {
            this.RouteMessage(serialMessage);
        }

        protected virtual void OnAborted()
        {

        }

        protected virtual void OnChildrenFinished()
        {

        }

        public void RouteMessage(string serialMessage)
        {
            switch (this.Status)
            {
                case WorkflowState.STARTED:
                    this.OnMessage(serialMessage);
                    break;
                case WorkflowState.FINISHED:
                    if (this.WorkflowItem != null) //otherwise pass to current workflow item for routing.
                        this.WorkflowItem.Value.RouteMessage(serialMessage);
                    break;
            }
        }

        protected void UpdateStatus(WorkflowState newStatus)
        {
            this.Status = newStatus;
            this.SendEvent(newStatus);
        }

        protected void Abort()
        {
            this.OnAborted();
            this.UpdateStatus(WorkflowState.ABORTED);

            if(this.WorkflowItem != null)
            {
                this.WorkflowItem.Value.Abort();
                LinkedListNode<Workflow> abortTarget = this.WorkflowItem.Next;
                while(abortTarget != null)
                {
                    abortTarget.Value.Abort();
                    abortTarget = abortTarget.Next;
                }
            }
        }

        protected void FinishOrAdvance()
        {
            bool complete = false;

            if (this.WorkflowQueue.Count > 0) //queue isn't empty
            {
                if (this.WorkflowItem == null) //first item hasn't been activated
                {
                    this.WorkflowItem = WorkflowQueue.First;
                    this.WorkflowItem.Value.Start(this.Workflow_onStateChange);
                }
                else //first item has been activated
                {
                    if (this.WorkflowItem.Next != null) //queue has a next item
                    {
                        this.WorkflowItem = this.WorkflowItem.Next;
                        this.WorkflowItem.Value.Start(this.Workflow_onStateChange);
                    }
                    else
                    {
                        complete = true;
                    }
                }
            }
            else
                complete = true;

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
    }

    public enum WorkflowState
    {
        PENDING,
        STARTED,
        FINISHED,
        ABORTED
    }

    public delegate void WorkflowStateChangeHandler(object sender, WorkflowState newState);
}
