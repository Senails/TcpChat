using System;
using System.Threading.Tasks;

static class AsyncLib {
    //отложеный запуск с возможностью отмены
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
    //интервальный запуск до отмены
    static public Action setInterval(Action action,int ms){
        bool flag = false;

        Func<Task> task = async ()=>{
            while (true){
                if (flag) return;
                await Task.Delay(ms);
                action(); 
            }  
        };
        task();

        return ()=>{
            flag = true;
        };
    }
    static public Action setInterval(AsyncAction action,int ms){
        bool flag = false;

        Func<Task> task = async ()=>{
            while (true){
                if (flag) return;
                await Task.Delay(ms);
                await action(); 
            }  
        };
        task();

        return ()=>{
            flag = true;
        };
    }
    //таска что бы заставить процесс жить
    // static public dontCloseRecord dontCloseProcces(){
    //     TaskCompletionSource endEmiter = new TaskCompletionSource();

    //     Action actionForClose = ()=>{
    //         endEmiter.SetResult();
    //     };

    //     Task closeTask = endEmiter.Task;

    //     return new dontCloseRecord(closeTask,actionForClose);
    // }


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
