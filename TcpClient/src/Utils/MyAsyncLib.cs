static class AsyncLib {
    static public Action setTimeout(AsyncAction action,int ms){
        bool flag = false;

        Func<Task> task = async ()=>{
            await Task.Delay(ms);
            if (flag) return;
            await action();   
        };
        task();

        return ()=>{
            flag = true;
        };
    }
    static public Action setTimeout(Action action,int ms){
        bool flag = false;

        Func<Task> task = async ()=>{
            await Task.Delay(ms);
            if (flag) return;
            action();   
        };
        task();

        return ()=>{
            flag = true;
        };
    }
    static public dontCloseRecord dontCloseProcces(){
        TaskCompletionSource endEmiter = new TaskCompletionSource();

        Action actionForClose = ()=>{
            endEmiter.SetResult();
        };

        Task closeTask = endEmiter.Task;

        return new dontCloseRecord(closeTask,actionForClose);
    }


    public delegate Task AsyncAction();
    public record dontCloseRecord {
        public Task closeTask; 
        public Action actionForClose;

        public dontCloseRecord (Task closeTask , Action actionForClose){
            this.closeTask = closeTask;
            this.actionForClose = actionForClose;
        }
    };
}
