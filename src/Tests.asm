global  start
extern ExitProcess
section .text
start:
    push rbp
    mov rbp, rsp
    sub rsp, 32
    mov edx, 30
    mov dword [rbp-4], edx
    mov edx, 20
    mov dword [rbp-8], edx
    mov edx, 10
    mov dword [rbp-12], edx
    mov edx, 1
    mov dword [rbp-16], edx
    lea rax, qword [rbp-3]
    mov qword [rbp-24], rax
    mov rax, qword [rbp-24]
    mov ebx, dword [rbp-16]
    mov edx, 4
    imul ebx, rdx, rdx //
    add rax, ebx //
    mov qword [rbp-32], rax
    mov edx, dword [rbp-16]
    mov rcx, qword [rbp-32]
    mov dword [rcx-0], edx
    xor ecx, ecx
    sub rsp, 32
    call ExitProcess