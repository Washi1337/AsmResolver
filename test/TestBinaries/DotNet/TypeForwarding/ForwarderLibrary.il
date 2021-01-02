.assembly ForwarderLibrary {}

.assembly extern ActualLibrary
{
}

.class extern forwarder IMyModel
{
	.assembly extern ActualLibrary
}

.class extern forwarder MyClass
{
	.assembly extern ActualLibrary
}