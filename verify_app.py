import requests
import re
from urllib.parse import urljoin

base = 'http://localhost:5148'


def fetch(session, path):
    url = urljoin(base, path)
    r = session.get(url, timeout=20, verify=False)
    return r.status_code, url, r.text


def main():
    print('START')
    session = requests.Session()
    session.verify = False
    requests.packages.urllib3.disable_warnings(requests.packages.urllib3.exceptions.InsecureRequestWarning)
    for path in ['/', '/Account/Login']:
        code, url, body = fetch(session, path)
        print(path, code, 'login_form' if 'login' in body.lower() else 'no-login-text')

    code, url, body = fetch(session, '/Account/Login')
    token_match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body)
    print('token_found', bool(token_match))
    if not token_match:
        raise SystemExit(1)

    login_data = {
        'UserName': 'admin',
        'Password': 'Admin@123',
        'ReturnUrl': '',
        '__RequestVerificationToken': token_match.group(1),
    }

    post = session.post(urljoin(base, '/Account/Login'), data=login_data, timeout=20)
    print('login_return', post.status_code, post.url)
    print('login_redirected', post.history and [r.status_code for r in post.history] or [])
    print('login_has_logout', 'logout' in post.text.lower())

    for path in ['/Employees', '/Finance/Projects', '/Transactions', '/DailyWork/MyUpdates', '/AuditLogs', '/Reports/Revenue']:
        r = session.get(urljoin(base, path), timeout=20)
        print(path, r.status_code, 'OK' if r.status_code == 200 else 'FAIL')


if __name__ == '__main__':
    main()
