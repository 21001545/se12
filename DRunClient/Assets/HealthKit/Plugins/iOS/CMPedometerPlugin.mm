#include <CoreMotion/CoreMotion.h>

extern "C" {

    typedef void (*InitCallback)(bool result);
    typedef void (*StepCallback)(int caller_id,bool result,float value);

    CMPedometer* pedometer;
    float step_count;
    float last_queried_step_count;

    void initializeCMPedometerPlugin(InitCallback callback)
    {
        pedometer = [[CMPedometer alloc] init];
        step_count = 0;
        last_queried_step_count = 0;
    
        if( CMPedometer.isStepCountingAvailable == false)
        {
            callback(false);
        }
        else
        {
            NSDate* startDate = [[NSDate alloc] init];
            [pedometer startPedometerUpdatesFromDate:startDate withHandler:^(CMPedometerData* pedometerData,NSError* error){
                if(error){
                    NSLog(@"error:%@", error);
                }
                else
                {
                    NSLog(@"step:startDate[%@] endDate[%@] steps[%@]", [pedometerData startDate], [pedometerData endDate], [pedometerData numberOfSteps]);

                    step_count = [[pedometerData numberOfSteps] floatValue];
                }
            }];
            
            callback(true);
        }
    }

    void cmp_readStepCountLive(int caller_id,StepCallback callback)
    { 
        float delta = step_count - last_queried_step_count;
        last_queried_step_count = step_count;
        callback(caller_id,true, delta);
    }

    void cmp_readStepCountPast(long beginTimestamp,long endTimestamp,int caller_id,StepCallback callback)
    {
        NSDate* startDate = [NSDate dateWithTimeIntervalSince1970:(NSTimeInterval)(beginTimestamp / 1000.0)];
        NSDate* endDate = [NSDate dateWithTimeIntervalSince1970:(NSTimeInterval)(endTimestamp / 1000.0)];

        [pedometer queryPedometerDataFromDate:startDate toDate:endDate withHandler:^(CMPedometerData* pedometerData,NSError* error){

            if(error){
                NSLog(@"error:%@", error);
                callback(caller_id,false,0);   
            }
            else
            {
                float value = [[pedometerData numberOfSteps] floatValue];

                callback(caller_id,true,value);
            }

        }];
    }    

}
