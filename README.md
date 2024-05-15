# 포트폴리오용 프로젝트

> [!NOTE]
> 지속적으로 스크립트 갱신중...
> <br>README도 지속적으로 갱신중...

> [!IMPORTANT]
> ![Code_2024-05-15_14-33-56](https://github.com/KimLiquid/MyProject/assets/114733076/f7b9e241-61fc-4e1d-a5bd-2718d28f8beb)
> <br>안쓰는 코드 중 일부는 이런식으로 안지우고 주석으로 처리해놔서 스크립트 보기 좀 더러울 수 있음

## 사용 스크립트 

### Player가 사용하는 스크립트
![Unity_2024-05-15_15-45-26](https://github.com/KimLiquid/MyProject/assets/114733076/fec51a56-94d4-49cc-a1de-f4ec3b2589ff) 
<br>Character 안에는 매쉬 랜더러, 리깅이 들어감
<br>Camera 안에는 FPS시점과 TPS시점의 카메라가 들어감
<br>![Unity_2024-05-15_15-49-42](https://github.com/KimLiquid/MyProject/assets/114733076/9cd8693b-a3c7-4ea9-9e48-1ce14a1cf28d)
![Unity_2024-05-15_15-49-31](https://github.com/KimLiquid/MyProject/assets/114733076/18fb3eb4-f98e-4a7f-9fba-8057c3e05031)

### Enemy가 사용하는 스크립트
![Unity_2024-05-15_18-23-39](https://github.com/KimLiquid/MyProject/assets/114733076/9f3eac09-3b5b-4dee-8d35-34d44ae908b0)
<br>※ EnemyDefinition.cs는 현재 사용하지않음 (들어가보면 모든 코드가 주석처리되어있음)<br>
![Unity_2024-05-15_18-36-38](https://github.com/KimLiquid/MyProject/assets/114733076/d18a5553-de2a-4301-afc7-c1a4a7582ae7)

## 스크립트 기능 설명

### Player쪽 기능
![Unity_2024-05-15_22-25-56](https://github.com/KimLiquid/MyProject/assets/114733076/e4fa73c0-1da0-4661-9fb1-7550c88a66e4)
<br>**컴포넌트에 카메라를 넣어놔야 함[^1]**
[^1]:안 넣어 두면 오류발생 

![Unity_2024-05-15_20-20-16](https://github.com/KimLiquid/MyProject/assets/114733076/e3157a2c-aaf9-4b1d-bfb2-fa932af6ec38)
<br>W - 앞으로 이동
<br>S - 뒤로 이동
<br>A - 왼쪽으로 이동
<br>D - 오른쪽으로 이동

![Unity_2024-05-15_20-41-40](https://github.com/KimLiquid/MyProject/assets/114733076/4e855963-7692-4ea2-8621-c49b4e9f4b51)
<br>왼쪽 쉬프트 - 대쉬
<br>왼쪽 컨트롤 - 걷기

![Unity_2024-05-15_20-44-19](https://github.com/KimLiquid/MyProject/assets/114733076/eb6747d3-3434-4323-aa6f-924d892550c6)
![Unity_2024-05-15_20-47-32](https://github.com/KimLiquid/MyProject/assets/114733076/da6e8437-567f-4e1e-aaa1-d34b11ba93fa)
<br>스페이스바 - 점프
<br>마우스 좌측 아래버튼[^2] - 구르기[^3]
[^2]:사진에서는 왼쪽 알트키를 사용
[^3]:아직 구르기에 무적은 없음, 구른 후 약간의 쿨타임이 있음

![Unity_2024-05-15_21-16-44](https://github.com/KimLiquid/MyProject/assets/114733076/13f0de75-6457-4133-9210-f2716502a68b)
<br>F5 - TPS <-> FPS 시점 변경
<br>.(점) - 커서 표시[^4]
[^4]:커서 표시중에는 카메라 회전 불가능

![Unity_2024-05-15_21-24-20](https://github.com/KimLiquid/MyProject/assets/114733076/827df6d7-f5cc-4864-8e35-da5d3ef27745)
<br>휠 위/아래 - 화면 확대/축소[^5]
[^5]:시점이 TPS일때만 확대/축소 가능

![Unity_2024-05-15_21-29-25](https://github.com/KimLiquid/MyProject/assets/114733076/b7b3b6b1-a621-43e7-8eb0-90cbe1e93ca0)
<br>탭 - 카운터 자세 토글[^6]
<br>카운터 자세에서 왼쪽/휠/오른쪽 클릭 - 상단/중단/하단 카운터[^7]
[^6]:이 상태에서는 움직이지 못함, 화면 확대/축소 불가, TPS/FPS시점 변경 불가
[^7]:아직 모션만 있고 카운터 판정은 없음)

![Unity_2024-05-15_21-41-38](https://github.com/KimLiquid/MyProject/assets/114733076/5b433ce5-924e-4706-a67a-7145b9a2699a)
<br>F - 등에 있는 칼을 발도/납도[^8]
<br>발도 상태에서 왼클릭 - 공격[^9]
<br>공중에서 왼클릭 - 점프 공격
[^8]:발도 상태에서는 패링 자세 불가능, 총을 꺼낼 수 없음
[^9]:10가지 모션중 랜덤으로 재생

![Unity_2024-05-15_21-55-30](https://github.com/KimLiquid/MyProject/assets/114733076/207a974a-1c71-4e98-b433-de359a6488b0)
<br>G - 총 꺼내기/집어넣기[^10]
<br>총을 꺼낸 상태에서 우클릭 - 조준/조준해제[^11]
<br>조준 중에 왼클릭 - 총 발사[^12]
<br>**모든공격은 아직은 적에게 공격 판정이 없음**
[^10]:총을 꺼낸 상태에서는 패링 자세 불가능, 발도 할 수 없음
[^11]:조준 상태에서는 조준을 풀기전 까진 위에 행동 포함 점프/구르기/총집어넣기/화면 확대/축소 불가능, FPS상태에서도 강제로 TPS시점으로 바뀜
[^12]:아직 총알이 나가진 않음

### Enemy쪽 기능
#### https://i.vgy.me/1nb39J.gif 순찰 모드
타겟을 찾을 때 까지 순찰[^13]
<br>랜덤한 각도로 돌고 벽이나 올라갈 수 없는 경사를 만나거나 **순찰 시간**<sub>설정한 범위 내의 랜덤한 시간이 지날</sub> 동안 직전함
<br>벽이나 올라갈 수 없는 경사를 만나거나 **순찰 시간**<sub>설정한 범위 내의 랜덤한 시간</sub>이 지나서 멈췄을 경우 **순찰 쿨타임**<sub>설정한 범위 내의 랜덤한 시간이 지난</sub> 이후 다시 랜덤한 각도로 돌고 직진[^14]
<br>돌기만 하고 움직이지 않을 시 **순찰 쿨타임**<sub>설정한 범위내의 랜덤한 시간</sub>에 설정한 만큼 곱하고 대기
[^13]:어그로타입이 있어서 적대일때만 추격하도록 설정할려했는데 깜빡하고 빠트림
[^14]:돌기만 하고 움직이지 않을 수 있음

#### https://i.vgy.me/s8I9V4.gif 추격 모드
감지범위 안 감지 각도 안에 타겟(플레이어) 이 들어가면 순찰모드가 끝나고 추격모드가 시작됌
<br>추격모드가 시작되면 설정한 만큼 감지범위가 늘어나고 설정한 만큼 이동속도가 곱해짐
<br>이후 타겟(플레이어) 을 추격함
<br>감지범위 바깥으로 이탈하면 감지범위, 이동속도가 다시 원상태로 돌아오고 일정시간 가만히 있다가 추격모드가 끝나고 순찰모드가 시작됌
<br>**아직 플레이어가 근처에 있을때 공격하진 않음**
