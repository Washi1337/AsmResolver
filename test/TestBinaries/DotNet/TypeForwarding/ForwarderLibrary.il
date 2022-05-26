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

.class extern forwarder MyNestedClass
{
	.class extern MyClass
}

.class extern forwarder MyClass2
{
	.assembly extern ActualLibrary
}

.class extern forwarder MyNestedClass
{
	.class extern MyClass2
}
