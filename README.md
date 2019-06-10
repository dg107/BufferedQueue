# BufferedQueue
This is a way to buffer up items into a queue as you wait for resoruces to process the items.  Each time the queue is read, every item in the Queue is returned as a list.  This allows you to process all the items and while processing new requests are added to the Queue to be processed togeather.
