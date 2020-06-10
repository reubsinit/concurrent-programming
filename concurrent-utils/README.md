# Concurrent-Utils
Concurrent utilities written in C#

The utilities offered within the BitNak concurrency library provide the functionality required to implement concurrency in concurrent programs.

## Active Object
- Utility to control life cycle of a single thread.

## Channel Based Active Object
- A ChannelBasedActiveObject serves as a Channel managed by an ActiveObject.

## Semaphore
- Utility used to control access to activity of a thread. Access to activity is controlled by tokens. A thread may act if it can acquire a token.

## FiFoSemaphore
- Utility used to control access to activity of a thread. Works on a first in first out principle. Access to activity is controlled by tokens. A thread may act if it can acquire a token.

## Mutex
- Utility used to control access to activity of a thread. Access to activity is controlled by a single token. A thread may act if it can acquire the token.

## Barrier
- Utility used to synchronize activity of a set number of threads.

## Rendezvous
- Utility used to synchronize activity of two threads.

## Latch
- Utility used to synchronize activity of an undefined number of threads. All threads waiting on the Latch will only continue with activity when the latch is released.

## Switch
- Utility used to allow exclusive access to activity permission.

## Read Write Lock
- Utility used to control thread read and write access. Ensures that multiple threads may read at any given time, while only one thread may write. No write permission will be granted if read permission has been acquired. No read permission will be granted if write permission has been acquired.

## Channel
- A Channel serves as a thread safe message passing pipeline.

## Bounded Channel
- A BoundedChannel serves as a thread safe message passing pipeline with a maximum number of messages that may exist on the BoundedChannel at any given time.
