def message = "";
pipeline {
    agent any
    
    environment {
        ImageName = "harbor.rpgp.cc/tgs/log-api"
        BuildConfiguration = 'Debug'
    }

    stages {
        stage('Prepare') {
            steps {
                sh "echo 'Branch: $GIT_BRANCH'";

                script {
                    switch(env.GIT_BRANCH)
                    {
                        case ['main', 'origin/main']:
                            env.kube_ns = "prod";
                            env.k8s_config = 'stag-k8s-config';
                            env.autoscaling = 'true';
                            break;
                        case ['release', 'origin/release']:
                            env.kube_ns = "staging";
                            env.k8s_config = 'stag-k8s-config';
                            env.autoscaling = 'false';
                            break;
                        default:
                            env.kube_ns = "dev";
                            env.k8s_config='k8s-kube-config'
                            env.autoscaling = 'false';
                            break;
                    }
                }

                configFileProvider([
                    configFile(
                        fileId: 'tgs.' + env.kube_ns , 
                        variable: 'tgsconfig')]) {
                    script {
                        def envItems = readProperties file: "$tgsconfig";

                        envItems.each { key, value -> 
                            env."tgs_${key}" = value;
                        }

                        env.TAG = env.kube_ns + '-' + sh(script: "echo `date +%Y%m%d%H%M`", returnStdout: true).trim()
                        message += "\nBranch: ${env.GIT_BRANCH}" 
                        message += "\nTag: ${env.TAG}"
                        message += "\nEnv: ${env.kube_ns}"
                    }
                }

                sh "echo tag: $TAG"
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build -c $BuildConfiguration'
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test -c $BuildConfiguration -l trx --no-build'
            }
        }
        
        stage('Push packages') {
            when {
                expression {
                    return env.kube_ns == 'dev';
                }
            }

            steps {
                sh 'dotnet pack -c $BuildConfiguration "Tgs.Logging/Tgs.Logging.csproj" --no-restore'
                sh 'dotnet nuget push "Tgs.Logging/bin/$BuildConfiguration/*.nupkg" -s "raypro-baget" -k admin --skip-duplicate'
            }
        }

        stage('Building docker image') {
            steps {
                sh 'dotnet build "LogApi" -c $BuildConfiguration -t:ContainerBuild -p:DockerfileTag="$ImageName:${TAG}" --no-restore '
            }
        }

        stage('Push docker image') {
            steps {
                sh 'docker push $ImageName:${TAG}'
            }
        }

        stage('Deploy') {
            when {
                expression {
                    return env.kube_ns == 'prod';
                }
            }

            steps {
                script {
                    input message: 'Ready to go?', ok: 'yes';
                }
                
                sh "echo start deploy"
            }
        }

        stage('Deploy log-api') { 
            environment {
                resource = getResources(env.k8s_config,env.kube_ns,"log-api")
            }
            steps {
                withKubeConfig([credentialsId: env.k8s_config]) {
                    withCredentials([string(credentialsId: "tgs-${kube_ns}-connectionstring", variable: "tgs_tgs")]) {
                        sh '''
                            cat << EOF | yq e -P | helm template deployment/k8s/log-api \
                                --set image.tag=${TAG} \
                                --set namespace=${kube_ns} \
                                --set connectionStrings.redis="$tgs_redis" \
                                --set nats.urls="$tgs_nats" \
                                --set autoscaling.enabled=${autoscaling} \
                                --set aspNetCoreEnvironment="$tgs_aspNetCoreEnvironment" \
                                --set notifyPath="$tgs_notifyPath"
                                -f -
                            {
                                "resources": $resources
                            }
                        '''
                        sh '''
                            cat << EOF | yq e -P | helm template deployment/k8s/log-api \
                                --set image.tag=${TAG} \
                                --set namespace=${kube_ns} \
                                --set connectionStrings.redis="$tgs_redis" \
                                --set nats.urls="$tgs_nats" \
                                --set autoscaling.enabled=${autoscaling} \
                                --set aspNetCoreEnvironment="$tgs_aspNetCoreEnvironment" \
                                --set notifyPath="$tgs_notifyPath" \
                             -f - | kubectl -n ${kube_ns} apply -f -
                            {
                                "resources": $resources
                            }
                        '''
                    }
                }
            }
        }

        stage('Add Tag') {
            when {
                expression {
                    return env.kube_ns == 'prod';
                }
            }		
            steps {
                sshagent(['ssh-jenkins']) {
                    sh "git tag -a ${TAG} -m ${TAG}"
                    sh "git push ${GIT_URL} ${TAG} HEAD:${GIT_BRANCH}"
                }
            }
        }
    }

    post {
        success {
             sendTelegram("Pipeline: ${currentBuild.fullDisplayName} OK! ${message} \n${currentBuild.getBuildCauses()[0].shortDescription}\n")
        }
        failure {
            sendTelegram("<b>Failed Pipeline:</b> ${currentBuild.fullDisplayName} Something is wrong with ${env.BUILD_URL}/console\n${currentBuild.getBuildCauses()[0].shortDescription}\n")
        }

        always {
            step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: false, keepLongStdio: true])
        }
    }
}

def sendTelegram(message) {
    def encodedMessage = URLEncoder.encode(message, "UTF-8")

    withCredentials([string(credentialsId: "telegramToken", variable: "TOKEN"), string(credentialsId: "telegramChatId", variable: "CHAT_ID")]) {

        response = httpRequest (consoleLogResponseBody: true,
                contentType: 'APPLICATION_JSON',
                httpMode: 'GET',
                url: "https://api.telegram.org/bot${TOKEN}/sendMessage?text=${encodedMessage}&chat_id=${CHAT_ID}",
                validResponseCodes: '200')
        return response
    }
}
