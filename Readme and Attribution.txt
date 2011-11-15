README
=========================
The code provided is not fully functional and does not yet distribute calls or data between
instances.  However, it does demonstrate the post-processor hooking and intercepting
properties and methods on classes marked as Distributed, which is the only large necessary
step before distributed processing.

ATTRIBUTION
=========================
The following open source projects are being used:
* Data4 (from Trust4; http://code.google.com/p/trust4/)
* InjectModuleInitializer (http://einaregilsson.com/2009/12/16/module-initializers-in-csharp/)
* Mono.Cecil (from Mono)

The following closed source components are being used:
* System.Threading (from Microsoft Reactive Extensions)

REACTIVE EXTENSIONS
=========================
Data4 contains the System.Threading DLL so it should out-of-the-box, however, if the Data4
library will not initialize correctly, you may need to install Reactive Extensions as per
the directions set out in "Getting Reactive Extensions.txt".

LICENSE
=========================
The code is open sourced and will most likely be available under an Apache license, however,
reliance on third-party open source libraries has not yet been fully determined due to the
incomplete nature of the project and the selected open source license may change due to
library licensing requirements before the completion of the project (in any event, this will
not affect the license of programs which use Process4 in any way).