#!/bin/sh
for f in /proc/[0-9]*/cmdline; do
  echo "$f:"
  cat $f | tr '\0' ' '
  echo
done