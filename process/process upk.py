import os
import subprocess
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor

def find_upk_files(folder):
    upk_files = []
    for file in Path(folder).rglob('*.upk'):
        upk_files.append(file)
    return upk_files

def process_upk_file(upk_file, exe_path):
    command = f'"{exe_path}" "{upk_file}" -console -silent -export=classes'
    subprocess.run(command, shell=True)

def process_upk_files_multithreaded(upk_files, exe_path, batch_size=10):
    with ThreadPoolExecutor() as executor:
        for i in range(0, len(upk_files), batch_size):
            batch = upk_files[i:i + batch_size]
            executor.map(process_upk_file, batch, [exe_path] * len(batch))

if __name__ == '__main__':
    folder = "D:/Mirror's Edge/TdGame"
    exe_path = "D:/git/UE-Explorer/UE Explorer/bin/Debug/UE Explorer.exe"
    upk_files = find_upk_files(folder)
    process_upk_files_multithreaded(upk_files, exe_path)
