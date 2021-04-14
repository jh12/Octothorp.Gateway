# Octothorp.Gateway

## 🔀 Reverse Proxy
Microsoft YARP *(YARP: A Reverse Proxy)* provides the reverse proxy facility of the gateway. Configuration examples can be found in the [documentation](https://microsoft.github.io/reverse-proxy/articles/configfiles.html)

[GitHub repository](https://github.com/microsoft/reverse-proxy)

## 🥬 Lettuce Encrypt
Lettuce Encrypt can be leveraged to obtain a LetsEncrypt SSL/TSL certificate  
[GitHub repository](https://github.com/natemcmaster/LettuceEncrypt)

```json
"LettuceEncrypt":{
    "AcceptTermsOfService": true,
    "DomainNames": [ "example.com", "www.example.com" ],
    "EmailAddress": "name@example.com",
    "UseStagingServer": false
}
```
