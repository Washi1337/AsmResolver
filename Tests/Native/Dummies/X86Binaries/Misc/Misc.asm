use32

das
daa
aas
aaa

inc eax
inc ecx
inc edx
inc ebx
inc esp
inc ebp
inc esi
inc edi

dec eax
dec ecx
dec edx
dec ebx
dec esp
dec ebp
dec esi
dec edi

push eax
push ecx
push edx
push ebx
push esp
push ebp
push esi
push edi

pop eax
pop ecx
pop edx
pop ebx
pop esp
pop ebp
pop esi
pop edi

pushad
popad

push 0x1337
push 0x15

nop
xchg ecx, eax
xchg edx, eax
xchg ebx, eax
xchg esp, eax
xchg ebp, eax
xchg esi, eax
xchg edi, eax

cwde
cdq

wait
pushfd
popfd
sahf
lahf

mov al, byte [0x1337]
mov eax, dword [0x1337]

mov byte [0x1337], al
mov dword [0x1337], eax

movsb
movsd
cmpsb
cmpsd

test al, 0x5
test eax, 0x1337

stosb
stosd
lodsb
lodsd
scasb
scasd

retn 0x1234
retn

int3
int 0x2
into
iretd

in al, 0x3
in eax, 0x3
out 0x3, al
out 0x3, eax

in al, dx
in eax, dx
out dx, al
out dx, eax

int1
hlt
cmc
clc
stc
cli
sti
cld
std

