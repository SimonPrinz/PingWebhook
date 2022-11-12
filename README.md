# PingWebhook

This program pings an ip address and sends notifications via [ntfy.sh](https://ntfy.sh/).

## Usage

```shell
# quick start to monitor cloudflare dns
docker run --rm ghcr.io/simonprinz/pingwebhook:latest \
  # use ntfy.sh with pingwebhook as topic
  --ntfyBaseUrl=https://ntfy.sh \
  --ntfyTopic=pingwebhook \
  1.1.1.1
```

```shell
# all arguments
docker run --rm ghcr.io/simonprinz/pingwebhook:latest \
  # set this to your ntfy instance
  --ntfyBaseUrl=https://ntfy.where-ever-you-are.com \
  # required if have a protected instance (via basic auth)
  --ntfyAuthentication=username:password \
  --ntfyTopic=pingwebhook \
  # interval in ms to wait between checks
  --checkInterval=5000 \
  # interval in ms to wait for ping response
  --checkTimeout=3000 \
  # custom title for ntfy notifications
  --title="Cloudflare DNS" \
  # verbose output for debugging
  --verbose \
  1.1.1.1
```
