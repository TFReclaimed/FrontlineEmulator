#!/bin/bash

if [[ $EUID -ne 0 ]]; then
   echo "This script must be run as root."
   exit 1
fi

firewall-cmd --add-service=http
firewall-cmd --add-service=https