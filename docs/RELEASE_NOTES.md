# Presence 1.0.5

The internet speed test now measures download for at least 2.5 seconds and upload for at least 2.5 seconds. Successful tests therefore run for at least five seconds, plus latency measurement and request overhead. The old 8 MiB cutoff no longer ends fast-connection measurements early. Throughput uses total transferred bytes divided by actual elapsed time.

Closing the dialog cancels the test. The existing 35-second timeout remains. Tests run only when requested and do not save results or send Presence device data.

One production build; no automated or live network tests requested.
