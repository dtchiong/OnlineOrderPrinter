﻿using OnlineOrderPrinter.Actions;
using OnlineOrderPrinter.Apis;
using OnlineOrderPrinter.Apis.Responses;
using OnlineOrderPrinter.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlineOrderPrinter.Sagas {
    class EventSagas : CancellableSaga {

        public static int FetchingCurrentEvents = 0;
        public static int FetchPastEventsTaskCount = 0;

        public static CancellationTokenSource CurrentFetchCurrentEventsCTS;
        public static CancellationTokenSource CurrentFetchPastEventsCTS;

        public static void FetchCurrentEvents(
            string restaurantId,
            string bearerToken,
            long? startEventId = null) {

            if (Interlocked.Exchange(ref FetchingCurrentEvents, 1) == 0) {
                Debug.WriteLine("Starting FetchCurrentEvents saga");

                CurrentFetchCurrentEventsCTS = new CancellationTokenSource();
                CtsPairMap.TryAdd(CurrentFetchCurrentEventsCTS.Token, CurrentFetchCurrentEventsCTS);

                Task.Run(() => {
                    CancellationToken cancellationToken = CurrentFetchCurrentEventsCTS.Token;
                    try {
                        FetchEventsResponse response = Api.FetchEvents(
                            restaurantId,
                            bearerToken,
                            startEventId,
                            null,
                            null,
                            cancellationToken).Result;
                        AppState.UserControlMainPage.Invoke((MethodInvoker)delegate {
                            if (response.IsSuccessStatusCode()) {
                                EventsContext eventsContext = startEventId != null ? EventsContext.Latest : EventsContext.CurrentDay;
                                EventActions.ReceiveEvents(response.Events, eventsContext);
                            }
                        });
                    } catch (AggregateException e) {
                        Debug.WriteLine(e.Message);
                    }
                    finally {
                        // Use the cancellation token to retrieve the cancellation token source so that we can dispose
                        if (CtsPairMap.TryRemove(cancellationToken, out CancellationTokenSource cancellationTokenSource)) {
                            cancellationTokenSource.Dispose();
                        }
                        Debug.WriteLine("Ended FetchCurrentEvents saga");
                        Interlocked.Exchange(ref FetchingCurrentEvents, 0);
                    }
                });
            }
        }

        public static void FetchPastEvents(
            string restaurantId,
            string bearerToken,
            long? startEventId = null,
            DateTime? startTime = null,
            DateTime? endTime = null) {

            if (FetchPastEventsTaskCount > 0) {
                CurrentFetchPastEventsCTS.Cancel();
            }

            Debug.WriteLine("Starting FetchPastEvents saga");
            Interlocked.Increment(ref FetchPastEventsTaskCount);
            CurrentFetchPastEventsCTS = new CancellationTokenSource();
            CtsPairMap.TryAdd(CurrentFetchPastEventsCTS.Token, CurrentFetchPastEventsCTS);

            Task.Run(() => {
                // Capture the token so that it's the one corresponding to the current task inside the task delegate
                CancellationToken cancellationToken = CurrentFetchPastEventsCTS.Token;
                try {
                    FetchEventsResponse response = Api.FetchEvents(
                        restaurantId,
                        bearerToken,
                        startEventId,
                        startTime,
                        endTime,
                        CurrentFetchPastEventsCTS.Token).Result;
                    AppState.UserControlMainPage.Invoke((MethodInvoker)delegate {
                        if (response.IsSuccessStatusCode()) {
                            EventActions.ReceiveEvents(response.Events, EventsContext.Past);
                        }
                    });
                } catch (AggregateException e) {
                    Debug.WriteLine(e.Message);
                }
                finally {
                    // Use the cancellation token to retrieve the cancellation token source so that we can dispose
                    if (CtsPairMap.TryRemove(cancellationToken, out CancellationTokenSource cancellationTokenSource)) {
                        cancellationTokenSource.Dispose();
                    }
                    Debug.WriteLine("Ended FetchPastEvents saga");
                    Interlocked.Decrement(ref FetchPastEventsTaskCount);
                }
            });
        }
    }
}
