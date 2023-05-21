A simple textfile collector for Prometheus node_exporter that gathers statistics about ZFS pools using zfs_influxdb utility. Only works with recent OpenZFS versions which have zfs_influxdb included.

It's C#, so it either requires .NET to be installed on the Linux machine (see https://learn.microsoft.com/en-us/dotnet/core/install/linux), or it can be built as a standalone executable (~15 Mb).

To use this textfile collector, add a cronjob for root user which redirects zfs_collector output to /var/lib/prometheus/node-exporter/zfs.prom, like this (to run the job every minute):

    */5 * * * * /usr/local/bin/zfs_collector >/var/lib/prometheus/node-exporter/zfs.prom

As all my monitored machines already run node_exporter, I find it redundant to build another standalone exporter for ZFS. However, such mofication should be staigthforward enough if needed.
