using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZR_MasterTools {

    public class OneTimeSignal {
        System.Action _callback;

        public void Connect (System.Action callback) {
            _callback += callback;
        }

        public void Remove (System.Action callback) {
            _callback -= callback;
        }

        public bool IsEmpty {get {return _callback == null;}}

        public void Clear () {
            _callback = null;
        }

        public void Call () {
            if (_callback == null) {
                return;
            }
            System.Action tmp = _callback;
            _callback = null;
            tmp();
        }
    }

    public class OneTimeSignal<T> {
        System.Action<T> _callback;

        public void Connect (System.Action<T> callback) {
            _callback += callback;
        }

        public void Remove (System.Action<T> callback) {
            _callback -= callback;
        }

        public bool IsEmpty {get {return _callback == null;}}

        public void Clear () {
            _callback = null;
        }

        public void Call (T t) {
            if (_callback == null) {
                return;
            }
            System.Action<T> tmp = _callback;
            _callback = null;
            tmp(t);
        }
    }

    public class OneTimeSignal<T1, T2> {
        System.Action<T1, T2> _callback;

        public void Connect (System.Action<T1, T2> callback) {
            _callback += callback;
        }

        public void Remove (System.Action<T1, T2> callback) {
            _callback -= callback;
        }

        public bool IsEmpty {get {return _callback == null;}}

        public void Clear () {
            _callback = null;
        }

        public void Call (T1 t1, T2 t2) {
            if (_callback == null) {
                return;
            }
            System.Action<T1, T2> tmp = _callback;
            _callback = null;
            tmp (t1, t2);
        }
    }

    public class OneTimeTimer {
        bool _isRunning = false;
        float _countDown = 0f;
        OneTimeSignal _signal = new OneTimeSignal();

        public System.Action Start (float duration, System.Action callback) {
            _isRunning = true;
            _countDown = duration;
            _signal.Connect (callback);
            return () => Clear();
        }

        public void Clear () {
            _isRunning = false;
            _countDown = 0f;
            _signal.Clear();
        }

        public bool Pause {
            get { return !_isRunning; }
            set { _isRunning = !value; }
        }

        public bool Update (float delta) {
            if (!_isRunning) {
                return true;
            }
            _countDown -= delta;
            if (_countDown <= 0f) {
                _isRunning = false;
                _countDown = 0f;
                _signal.Call();
                return true;
            }
            return false;
        }
    }

    public class OneTimeTimerManager {
        LinkedList<OneTimeTimer> _timerList = new LinkedList<OneTimeTimer>();

        public OneTimeSignal _updateSignal = new OneTimeSignal();

        float _scale = 1f;
        public float Scale {
            get { return _scale; }
            set { _scale = value; }
        }

        public OneTimeTimer Start (float t, System.Action callback) {
            OneTimeTimer timer = new OneTimeTimer ();
            _timerList.AddFirst (timer);
            timer.Start (t, callback);
            return timer;
        }

        public void Update () {
            _updateSignal.Call ();
            if (_timerList.Count <= 0) {
                return;
            }
            float delta = _scale * Time.deltaTime;
            LinkedListNode<OneTimeTimer> firstNode = _timerList.First;
            LinkedListNode<OneTimeTimer> node = _timerList.Last;
            while (node != firstNode) {
                bool ret = node.Value.Update (delta);
                if (ret) {
                    LinkedListNode<OneTimeTimer> tmp = node.Previous;
                    _timerList.Remove(node);
                    node = tmp;
                }
                else {
                    node = node.Previous;
                }
            }
            if (firstNode.Value.Update (delta)) {
                _timerList.Remove(firstNode);
            }
        }
    }

    public class ManualResetEvent {
        bool _set = false;
        OneTimeSignal _signal = new OneTimeSignal();

        public bool IsSet {
            get { return _set; }
            set {
                _set = value;
                if (value) {
                    _signal.Call ();
                }
            }
        }

        public OneTimeSignal Signal {get { return _signal; }}

        public ManualResetEvent (bool set = false) {
            _set = set;
        }
    }

    public class Continuation {
        IEnumerator _enumerator;
        bool _isComplete = false;

        System.Action _onComplete = null;
        public System.Action _onWaitExit = null;
        public System.Action _onExit = null;

        public bool IsComplete {get { return _isComplete; }}

        private Continuation (IEnumerator enumerator) {
            _enumerator = enumerator;
        }

        public static void ExitContinuation (Continuation cont) {
            if (null == cont) {
                return;
            }
            cont.Exit ();
            cont = null;
        }

        public void Exit () {
            bool tmp = _isComplete;
            _isComplete = true;
            _enumerator = null;
            _onComplete = null;
            if (_onWaitExit != null) {
                if (!tmp) {
                    _onWaitExit ();
                }
                _onWaitExit = null;
            }
            if (_onExit != null) {
                if (!tmp) {
                    _onExit();
                }
                _onExit = null;
            }
        }

        public static Continuation Start (IEnumerator enumerator) {
            return Start (enumerator, null);
        }

        public static Continuation Start (IEnumerator enumerator, System.Action completeCallback) {
            return Start(enumerator, completeCallback, null);
        }

        public static Continuation Start (IEnumerator enumerator, System.Action completeCallback, System.Action exitCallback) {
            Continuation continuation = new Continuation(enumerator);
            continuation._onComplete = completeCallback;
            continuation._onExit = exitCallback;
            continuation.Next();
            return continuation;
        }

        public delegate void YieldCallback (Continuation enumerator);
        public static System.Action<Continuation> Yield (YieldCallback callback) {
            return new System.Action<Continuation> (callback);
        }

        public void Next () {
            if (_isComplete) {
                return;
            }

            bool hasNext = _enumerator.MoveNext ();
            if (!hasNext) {
                _isComplete = true;
                if (_onComplete != null) {
                    _onComplete();
                }
                return;
            }
            if (_onWaitExit != null) {
                _onWaitExit = null;
            }
            if (_enumerator != null) {
                System.Action<Continuation> callback = _enumerator.Current as System.Action<Continuation>;
                if (callback == null) {
                    Debug.LogError("Continuation yield return null : " + _enumerator.ToString());
                }
                callback(this);
            }
        }

        public static System.Action<Continuation> WaitForSignal (OneTimeSignal signal) {
            return Continuation.Yield (
                (Continuation continuation) => {
                    System.Action f = () => continuation.Next();
                    signal.Connect(f);
                    continuation._onWaitExit += () => signal.Remove(f);
                }
            );
        }

        public static System.Action<Continuation> WaitForSignal<T>(OneTimeSignal<T> signal, System.Action<T> callback) {
            return Continuation.Yield (
                (Continuation continuation) => {
                    System.Action<T> f = (T v) => {
                        if (callback != null){
                            callback (v);
                        }
                        continuation.Next();
                    };
                    signal.Connect (f);
                    continuation._onWaitExit += () => signal.Remove(f);
                }
            );
        }

        public static System.Action<Continuation> WaitForSignal<T1, T2>(OneTimeSignal<T1, T2> signal, System.Action<T1, T2> callback) {
            return Continuation.Yield (
                (Continuation continuation) => {
                    System.Action<T1, T2> f = (T1 t1, T2 t2) => {
                        if (callback != null) {
                            callback(t1, t2);
                        }
                        continuation.Next();
                    };
                    signal.Connect(f);
                    continuation._onWaitExit += () => signal.Remove(f);
                }
            );
        }

        public static System.Action<Continuation> WaitForSeconds (float t, OneTimeTimerManager manager) {
            return Continuation.Yield (
                (Continuation continuation) => {
                    System.Action cancel = manager.Start (t, () => {
                        continuation._onWaitExit = null;
                        continuation.Next();
                    }).Clear;
                    continuation._onWaitExit += cancel;
                }
            );
        }

        public static IEnumerator WaitForSecondsTask (float t, OneTimeTimerManager mgr, System.Action callback = null) {
            yield return Continuation.WaitForSeconds (t, mgr);
            if (callback != null) {
                callback();
            }
        }

        public static System.Action<Continuation> WaitForNextContinuation (IEnumerator next) {
            return WaitForNextContinuation (next, null);
        }

        public static System.Action<Continuation> WaitForNextContinuation(IEnumerator next, System.Action exitCallback) {
            return Continuation.Yield (
                (Continuation continuation) => {
                    Continuation nextContinuation = Continuation.Start (next, () => continuation.Next(), exitCallback);
                    continuation._onWaitExit += () => nextContinuation.Exit();
                }
            );
        }

        public static System.Action<Continuation> WaitForAnyContinuation(params IEnumerator[] arrays) {
            return Continuation.Yield (
                (Continuation continuation) => {
                    Continuation[] nextArray = new Continuation[arrays.Length];
                    bool stop = false;
                    for (int i = 0; i < arrays.Length; i++)  {
                        int index = i;
                        Continuation nextContinuation = Continuation.Start (arrays[index], () => {
                            for (int j = 0; j < nextArray.Length; j++) {
                                if (j != index) {
                                    if (nextArray[j] != null) {
                                        nextArray[j].Exit();
                                    }
                                }
                            }
                            stop = true;
                            continuation.Next();
                        });
                        if (stop) {
                            return;
                        }
                        nextArray[i] = nextContinuation;
                    }

                    continuation._onWaitExit += () => {
                        for (int i = 0; i < nextArray.Length; i++) {
                            if (nextArray[i] != null) {
                                nextArray[i].Exit();
                            }
                        }
                    };
                }
            );
        }

        public static System.Action<Continuation> WaitForAllContinuation(params IEnumerator[] arrays) {
            return Continuation.Yield (
                (Continuation continuation) => {
                    Continuation[] nextArray = new Continuation[arrays.Length];
                    bool stop = false;
                    int count = 0;
                    int endCount = arrays.Length;
                    for (int i = 0; i < arrays.Length; i++) {
                        int index = i;
                        Continuation nextContinuation = Continuation.Start(arrays[index], () => {
                            count += 1;
                            if (count >= endCount) {
                                stop = true;
                                continuation.Next();
                            }
                        });
                        if (stop) {
                            return;
                        }
                        nextArray[i] = nextContinuation;
                    }

                    continuation._onWaitExit += () => {
                        for (int i = 0; i < nextArray.Length; i++) {
                            if (nextArray[i] != null) {
                                nextArray[i].Exit();
                            }
                        }
                    };
                }
            );
        }

        public static IEnumerator SignalTask(OneTimeSignal signal, System.Action callback = null) {
            yield return WaitForSignal (signal);
            if (callback != null) {
                callback();
            }
        }

        public static IEnumerator SignalTask<T>(OneTimeSignal<T> signal, System.Action<T> callback) {
            yield return WaitForSignal<T> (signal, callback);
        }

        public static IEnumerator SignalTask<T1, T2> (OneTimeSignal<T1, T2> signal, System.Action<T1, T2> callback) {
            yield return WaitForSignal<T1, T2>(signal, callback);
        }

        public static System.Action<Continuation> WaitForManualResetEvent (ManualResetEvent eve) {
            return Continuation.WaitForNextContinuation (WaitForManualResetEventTask(eve));
        }

        public static IEnumerator WaitForManualResetEventTask(ManualResetEvent eve, System.Action callback = null) {
            if (eve.IsSet) {
                if (callback != null) {
                    callback();
                }
                yield break;
            }
            yield return Continuation.WaitForSignal (eve.Signal);
            if (callback != null) {
                callback();
            }
        }
    }
}
