python -m venv venv
venv/Scripts/activate
python -m pip install --upgrade pip
pip3 install torch~=2.2.1 --index-url https://download.pytorch.org/whl/cu121
pip3 install grpcio
pip3 install six
python -m pip install mlagents
