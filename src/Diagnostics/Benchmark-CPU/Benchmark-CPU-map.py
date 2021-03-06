#v0.3

import sys, json, copy

def main():
    stdin = json.load(sys.stdin)
    job = stdin["Job"]
    nodesInfo = stdin["Nodes"]
    nodelist = job["TargetNodes"]

    if len(nodelist) != len(set(nodelist)):
        # Duplicate nodes
        raise Exception('Duplicate nodes')

    nodeOS = {}
    nodeSize = {}
    for nodeInfo in nodesInfo:
        node = nodeInfo["Node"]
        try:
            nodeSize[node] = json.loads(nodeInfo["Metadata"])["compute"]["vmSize"]
        except Exception as e:
            nodeSize[node] = "Unknown"
        try:
            distroInfo = nodeInfo["NodeRegistrationInfo"]["DistroInfo"]
            if "Ubuntu".lower() in distroInfo.lower():
                nodeOS[node] = "ubuntu"
            elif "CentOS".lower() in distroInfo.lower():
                nodeOS[node] = "centos"
            elif "Redhat".lower() in distroInfo.lower():
                nodeOS[node] = "redhat"
            else:
                nodeOS[node] = "other"
        except Exception as e:
            raise Exception("Can not retrive distroInfo from NodeRegistrationInfo of node {0}. Exception: {1}".format(node, e))

    commandInstallSysbenchOnUbuntu = "(apt install -y sysbench || apt update && apt install -y sysbench) >/dev/null 2>&1 && "
    commandInstallSysbenchOnCentos = "(yum install -y epel-release && yum install -y sysbench) >/dev/null 2>&1 && "
    commandInstallSysbenchOnRedhat = "(curl -s https://packagecloud.io/install/repositories/akopytov/sysbench/script.rpm.sh | bash && yum install -y sysbench) >/dev/null 2>&1 && "
    commandRunLscpu = "lscpu && "
    commandRunSysbench = "sysbench --test=cpu --num-threads=`grep -c ^processor /proc/cpuinfo` run >output 2>&1 && cat output"
    commandDetectDistroAndRun = ("cat /etc/*release > distroInfo && "
                                 "if cat distroInfo | grep -Fiq 'Ubuntu'; then ({});"
                                 "elif cat distroInfo | grep -Fiq 'CentOS'; then ({});"
                                 "elif cat distroInfo | grep -Fiq 'Redhat'; then ({});"
                                 "elif cat distroInfo | grep -Fiq 'Red Hat'; then ({});"
                                 "fi").format(commandInstallSysbenchOnUbuntu + commandRunSysbench, 
                                              commandInstallSysbenchOnCentos + commandRunSysbench,
                                              commandInstallSysbenchOnRedhat + commandRunSysbench,
                                              commandInstallSysbenchOnRedhat + commandRunSysbench)
    taskTemplate = {
        "Id":0,
        "CommandLine":None,
        "Node":None,
        "CustomizedData":None,
    }

    tasks = []
    id = 1
    for node in nodelist:
        task = copy.deepcopy(taskTemplate)
        task["Id"] = id
        id += 1
        task["Node"] = node
        task["CustomizedData"] = nodeSize[node]
        task["CommandLine"] = commandRunLscpu
        if nodeOS[node] == "ubuntu":
            task["CommandLine"] += commandInstallSysbenchOnUbuntu + commandRunSysbench
        elif nodeOS[node] == "centos":
            task["CommandLine"] += commandInstallSysbenchOnCentos + commandRunSysbench
        elif nodeOS[node] == "redhat":
            task["CommandLine"] += commandInstallSysbenchOnRedhat + commandRunSysbench
        else:
            task["CommandLine"] += commandDetectDistroAndRun
        tasks.append(task)

    print(json.dumps(tasks))

if __name__ == '__main__':
    main()
